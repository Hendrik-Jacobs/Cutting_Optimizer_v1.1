using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Cutting_Optimizer
{
    [Serializable]
    public class Calc
    {
        public List<Part> Parts { get; set; }
        public List<Part> PartsBar { get; set; }
        public ObservableCollection<Part> PartsSave { get; set; }
        public ObservableCollection<Board> BoardsSave { get; set; }
        public List<Board> Boards = new List<Board>();
        public List<Board> BoardList = new List<Board>();
        public List<Board> Bars = new List<Board>();
        public List<Board> BarList = new List<Board>();
        private List<List<int>> FullDeleted = new List<List<int>>();
        private List<List<int[]>> FullPositions = new List<List<int[]>>();
        private List<int> BestBar = new List<int>();

        // If Boards/Bars from stock are used, the indexes get saved in this List.
        public List<int> Used = new List<int>();

        private int Width;
        private int Height;
        public int Length;
        public int Thickness;
        public int BestLength;
        public int Space { get; set; }
        public int SpaceBar { get; set; }

        private int Stage;




        // Start search for solutions for 1D And 2D, if Boards and Parts avaible.
        public bool Make()
        {
            if (BoardList.Count > 0 && Parts.Count > 0)
                if (Run2D()) return true;
            
            if (BarList.Count > 0 && PartsBar.Count > 0)
                Run1D();

            return false;
        }





        private void Run1D()
        {
            var index = 0;
            var end = BarList.Count;

            // While Boards and Parts avaible.
            while (index < end && PartsBar.Count > 0)
            {
                // Continue (next Bar) if no Parts for this Bar avaible.
                if (!PartAvaible(BarList[index].Thickness, true, BarList[index].Width, BarList[index].Height))
                {
                    index++;
                    continue;
                }

                Bars.Add(new Board
                {
                    SqlIndex = BarList[index].SqlIndex,
                    Width = BarList[index].Width,
                    Height = BarList[index].Height,
                    Price = BarList[index].Price,
                    Price_string = BarList[index].Price_string,
                    Amount = BarList[index].Amount,
                    Unlimited = BarList[index].Unlimited,
                    Bar = BarList[index].Bar,
                    Thickness = BarList[index].Thickness
                });

                if (BarList[index].SqlIndex != -1)
                    Used.Add(BarList[index].SqlIndex);

                // Contains indexes of used parts while recursion.
                List<int> deleted = new List<int> ();

                // Actual Length and Thickness (Bar).
                Length = BarList[index].Width;
                Thickness = BarList[index].Thickness;

                BestLength = 0;
                var length = 0;
                Stage = 0;

                // Run recursion.
                Recursion1D(deleted, length);

                BestBar.Sort();
                BestBar.Reverse();

                // Add used parts to Bar and delete from List.
                foreach (int i in BestBar)
                {
                    Bars[Bars.Count - 1].Parts.Add(new Part(PartsBar[i]));
                    PartsBar.RemoveAt(i);
                }
                BestBar.Clear();
                

                // Delete Bar from List.
                if (!BarList[index].Unlimited) index++;
            }
        }



        private bool Recursion1D(List<int> deleted, int length)
        {
            Stage++;
            if (Stage > Options.MaxStage)  return true;

            List<Part> Tested = new List<Part> { };
            var i = -1;
            foreach (Part part in PartsBar)
            {
                i++;
                if (IsTested(part, Tested)) continue;

                if ((part.Thickness == 0 || part.Thickness == Thickness) && 
                    !deleted.Contains(PartsBar.IndexOf(part)))
                {
                    Tested.Add(part);
                    var lengthOld = length;
                    length = (length == 0) ? length + part.Width : length + part.Width + SpaceBar;
                    if (length <= Length)
                    {
                        deleted.Add(i);
                        if (Recursion1D(deleted, length)) return true;
                        deleted.RemoveAt(deleted.Count - 1);
                    }
                    length = lengthOld;
                }
            }

            if (length > BestLength)
            {
                BestLength = length;
                BestBar = new List<int> (deleted);
                // Stop Recursion if perfect solution found.
                if (BestLength == Length) return true;
            }
            return false;
        }



        // Check if Part with same shape is already in Tested List.
        private bool IsTested(Part part, List<Part> Tested)
        {
            foreach (Part test in Tested)
                if (Tools.CheckIfPartsEqual(part.Width, part.Height, part.Thickness, test.Width, test.Height, test.Thickness))
                        return true;
            return false;
        }



        // Check if Parts are avaible, which fit in board.
        private bool PartAvaible(int thickness, bool bar, int width, int height)
        {
            List<Part> PartsTemp = (bar) ? new List<Part> (PartsBar) : new List<Part> (Parts);
            foreach (Part part in PartsTemp)
            {
                if ((width >= part.Width || height >= part.Height) && 
                    (width >= part.Height || height >= part.Width)  && 
                    (part.Thickness == 0 || part.Thickness == thickness))
                        return true;
            }
            return false;
        }



        private bool Run2D()
        {
            int board_index = 0;


            // While Parts and Boards aviable.
            while (Parts.Count > 0 && BoardList.Count > 0)
            {
                // Continue if no Parts for actual Board avaible.
                if (!PartAvaible(BoardList[0].Thickness, false, BoardList[0].Width, BoardList[0].Height))
                {
                    BoardList.RemoveAt(0);
                    continue;
                }
                Width = BoardList[0].Width;
                Height = BoardList[0].Height;
                Thickness = BoardList[0].Thickness;

                Boards.Add(new Board
                {
                    SqlIndex = BoardList[0].SqlIndex,
                    Width = BoardList[0].Width,
                    Height = BoardList[0].Height,
                    Price = BoardList[0].Price,
                    Price_string = BoardList[0].Price_string,
                    Amount = BoardList[0].Amount,
                    Unlimited = BoardList[0].Unlimited,
                    Bar = BoardList[0].Bar,
                    Thickness = BoardList[0].Thickness
                });

                if (BoardList[0].SqlIndex != -1)
                    Used.Add(BoardList[0].SqlIndex);
                if (!BoardList[0].Unlimited) BoardList.RemoveAt(0);

                List<int> deleted = new List<int> { };
                List<int[]> positions = new List<int[]> { };
                List<int[]> bottomLeft = new List<int[]> { new int[] { 0, 0 } };

                Stage = 0;
                // Find solution(s).
                if (!Options.SimpleMode)
                {
                    Recursion(bottomLeft, deleted, positions);

                    // Choose best solution.
                    if (FullDeleted.Count > 1) FindBest();
                    else if (FullDeleted.Count == 0) return true;
                }
                else
                {
                    FullDeleted.Add(deleted);
                    FullPositions.Add(positions);
                }

                if ((Options.CloseHolesEnd || Options.SimpleMode) && Parts.Count > 0)
                    CloseHoles(0);

                // Add Parts to Board.
                int count = 0;
                foreach (int i in FullDeleted[0])
                {
                    Parts[i].XPosition = FullPositions[0][count][0];
                    Parts[i].YPosition = FullPositions[0][count][1];
                    Parts[i].Rotate = (FullPositions[0][count][2] == 1);

                    Boards[board_index].Parts.Add(new Part(Parts[i]));
                    count++;
                }


                // Delete used Parts from List.
                FullDeleted[0].Sort();
                FullDeleted[0].Reverse();
                List<Part> parts_deleted = new List<Part> { };

                foreach (int i in FullDeleted[0])
                {
                    parts_deleted.Add(new Part(Parts[i]));
                    Parts.RemoveAt(i);
                }
                FullDeleted.Clear();
                FullPositions.Clear();



                // Dublicate Board if possible.
                // Same shape and same parts avaible.
                while (true)
                {                    
                    if (!EnoughParts(parts_deleted)) break;

                    int index_found = -1;
                    foreach (Board board in BoardList)
                    {
                        if ((board.Width == Boards[Boards.Count - 1].Width && board.Height == Boards[Boards.Count - 1].Height) ||
                            (board.Width == Boards[Boards.Count - 1].Height && board.Height == Boards[Boards.Count - 1].Width))
                        {
                            index_found = BoardList.IndexOf(board);
                            break;
                        }
                    }
                    if (index_found < 0) break;

                    foreach (Part part in parts_deleted)
                        Parts.RemoveAt(FindPartIndex(part));

                    Boards.Add(new Board(Boards[Boards.Count - 1]));
                    Boards[Boards.Count - 1].Price = BoardList[index_found].Price;
                    Boards[Boards.Count - 1].Price_string = BoardList[index_found].Price_string;
                    Boards[Boards.Count - 1].Unlimited = BoardList[index_found].Unlimited;

                    Used.Add(BoardList[index_found].SqlIndex);

                    if (!BoardList[index_found].Unlimited)
                    {
                        BoardList.RemoveAt(index_found);
                    }
                    board_index++;                        
                }


                // Remove Board if no Parts set.
                if (Boards[board_index].Parts.Count == 0)
                {
                    Used.RemoveAt(Used.IndexOf(Boards[board_index].SqlIndex));
                    Boards.RemoveAt(board_index);
                }
                else board_index++;
            }
            return false;
        }



        private int FindPartIndex(Part part)
        {
            foreach (Part part2 in Parts)
            {
                if (Tools.CheckIfPartsEqual(part.Width, part.Height, part.Thickness, part2.Width, part2.Height, part2.Thickness))
                {
                    return Parts.IndexOf(part2);
                }
            }
            return 0;
        }



        private bool EnoughParts(List<Part> parts_deleted)
        {
            foreach (Part part in parts_deleted)
            {
                if (!CountParts(part, parts_deleted))
                {
                    return false;
                }
            }
            return true;
        }



        // Count a given part in parts_deleted and Parts.
        // Returns false, if less of these parts are in Parts.
        private bool CountParts(Part part, List<Part> parts_deleted)
        {
            int count_deleted = 0, count_Parts = 0;
            foreach (Part test in parts_deleted)
            {
                if (Tools.CheckIfPartsEqual(part.Width, part.Height, part.Thickness, 
                                            test.Width, test.Height, test.Thickness))
                {
                    count_deleted++;
                }
            }

            foreach (Part test in Parts)
            {
                if (Tools.CheckIfPartsEqual(part.Width, part.Height, part.Thickness, 
                                            test.Width, test.Height, test.Thickness))
                {
                    count_Parts++;
                    if (count_deleted == count_Parts) return true;
                }
            }
            return false;
        }




        private bool Recursion(List<int[]> bottomLeft, List<int> deleted, List<int[]> positions)
        {
            Stage++;
            if (Stage > Options.MaxStage) return true;

            int count = 0;
            int part_counter = -1;
            List<Part> Tested = new List<Part> { };

            foreach (Part part in Parts)
            {
                part_counter++;
                if (!(part.Thickness == 0 || part.Thickness == Thickness))
                    continue;

                if (!deleted.Contains(part_counter) && !IsTested(part, Tested))
                {
                    foreach (int[] position in bottomLeft)
                    {
                        bool alreadyUsed = false;
                        foreach (int[] p in positions)
                            if (position[0] == p[0] && position[1] == p[1])
                                alreadyUsed = true;
                        if (alreadyUsed) continue;


                        int x = position[0];
                        int y = position[1];

                        bool found = SetPart(part, deleted, positions, x, y, true);
                        if (found)
                        {
                            count++;

                            deleted = AddDeleted(deleted, part_counter);
                            positions = AddPosition(positions, position[0], position[1], 0);

                            List<int[]> newBottomLeft = new List<int[]>(bottomLeft);
                            newBottomLeft.RemoveAt(newBottomLeft.IndexOf(position));
                            newBottomLeft = FindNewBottomLeft(part.Width, part.Height, 
                                                              position, newBottomLeft, 
                                                              deleted, positions);

                            if (Recursion(newBottomLeft, deleted, positions))
                                return true;

                            deleted.RemoveAt(deleted.Count - 1);
                            positions.RemoveAt(positions.Count - 1);
                        }

                        // Rotate if sides are not equal
                        if (part.Width != part.Height)
                        {
                            found = SetPart(part, deleted, positions, x, y, false);
                            if (found)
                            {
                                count++;

                                deleted = AddDeleted(deleted, part_counter);
                                positions = AddPosition(positions, position[0], position[1], 1);

                                List<int[]> newBottomLeft = new List<int[]>(bottomLeft);
                                newBottomLeft.RemoveAt(newBottomLeft.IndexOf(position));
                                newBottomLeft = FindNewBottomLeft(part.Height, part.Width, position, newBottomLeft, deleted, positions);

                                if (Recursion(newBottomLeft, deleted, positions))
                                    return true;

                                deleted.RemoveAt(deleted.Count - 1);
                                positions.RemoveAt(positions.Count - 1);
                            }
                        }

                        Tested.Add(part);
                    }
                }
            }

            if (count == 0)
            {
                FullDeleted.Add(new List<int>(deleted));
                FullPositions.Add(new List<int[]>(positions));

                if (Options.CloseHolesEvery)
                    CloseHoles(FullPositions.Count - 1);
            }
            
            return false;
        }



        // Find possible positions for next part.
        private List<int[]> FindNewBottomLeft(int width, int height, int[] position, List<int[]> bottomLeft, List<int> deleted, List<int[]> positions)
        {
            var shift = 0;
            while (position[1] - shift >= 0)
            {
                if (position[0] + width + Space < Width - 1 &&
                    (position[1] - shift == 0 || IsOnTop(width, position, deleted, positions, shift)))
                {
                    int[] temp = { position[0] + width + Space, position[1] - shift };
                    if (!bottomLeft.Contains(temp))
                    {
                        bottomLeft.Add(temp);
                        break;
                    }
                }

                if (Options.Shift) shift++;
                else break;
            }

            shift = 0;
            while (position[0] - shift >= 0)
            {
                if (position[1] + height + Space < Height - 1 &&
                (position[0] == 0 - shift || IsOnRight(height, position, deleted, positions, shift)))
                {
                    int[] temp = { position[0] - shift, position[1] + height + Space };
                    if (!bottomLeft.Contains(temp))
                    {
                        bottomLeft.Add(temp);
                    }
                }

                if (Options.Shift) shift++;
                else break;
            }

            bottomLeft = bottomLeft.OrderBy(r => r[1]).ThenBy(r => r[0]).ToList();
            return bottomLeft;
        }




        private bool IsOnTop(int width, int[] position, List<int> deleted, List<int[]> positions, int shift)
        {
            if (position[1] - Space - shift >= 0)
            {
                var index = 0;
                foreach (int del in deleted)
                {
                    int partWidth = (positions[index][2] == 0) ? Parts[del].Width : Parts[del].Height;
                    int partHeight = (positions[index][2] == 0) ? Parts[del].Height : Parts[del].Width;

                    if (position[0] + width + Space >= positions[index][0] - Space &&
                        position[0] + width + Space < positions[index][0] + partWidth + Space&&
                        position[1] - Space - shift == positions[index][1] + partHeight)
                    {
                        return true;
                    }
                    index++;
                }
            }
            return false;
        }


        private bool IsOnRight(int height, int[] position, List<int> deleted, List<int[]> positions, int shift)
        {

            if (position[0] - Space - shift >= 0)
            {
                var index = 0;
                foreach (int del in deleted)
                {
                    int partWidth = (positions[index][2] == 0) ? Parts[del].Width : Parts[del].Height;
                    int partHeight = (positions[index][2] == 0) ? Parts[del].Height : Parts[del].Width;

                    if (position[1] + height + Space >= positions[index][1] - Space &&
                        position[1] + height + Space < positions[index][1] + partHeight + Space &&
                        position[0] - Space - shift == positions[index][0] + partWidth)
                    {
                        return true;
                    }
                    index++;

                }
            }
            return false;
        }


        private List<int> AddDeleted(List<int> deleted, int part_counter)
        {
            try
            {
                deleted.Add(part_counter);
            }
            catch
            {
                deleted = new List<int> { part_counter };
            }

            return deleted;
        }

        private List<int[]> AddPosition(List<int[]> positions, int x, int y, int rotate)
        {
            int[] to_add = new int[] { x, y, rotate };
            try
            {
                positions.Add(to_add);
            }
            catch
            {
                positions = new List<int[]> { to_add };
            }
            return positions;
        }



        // Set Part if no collision found.
        private bool SetPart(Part part, List<int> deleted, List<int[]> positions, int x, int y, bool vertical)
        {
            int partWidth, partHeight;

            if (vertical)
            {
                partWidth = part.Width;
                partHeight = part.Height;
            }
            else
            {
                partWidth = part.Height;
                partHeight = part.Width;
            }

            if (x + partWidth <= Width && y + partHeight <= Height && 
                !FindCollision(deleted, positions, x, y, partWidth, partHeight))
                    return true;
            return false;
        }



        // Find min and max x and y values of part on board including spcae.
        private int[] FindOutlines(Part part, int[] position)
        {
            int[] outlinesDeleted;
            if (position[2] == 0)
                outlinesDeleted = new int[] {
                    position[0] - Space,
                    position[0] + part.Width + Space,
                    position[1] - Space,
                    position[1] + part.Height + Space };
            else
                outlinesDeleted = new int[] {
                    position[0] - Space,
                    position[0] + part.Height + Space,
                    position[1] - Space,
                    position[1] + part.Width + Space };
            return outlinesDeleted;
        }



        private bool FindCollision(List<int> deleted, List<int[]> positions, int x, int y, int partWidth, int partHeight)
        {
            int index = 0;
            int[] outlines = new int[] { x, x + partWidth, y, y + partHeight };
            foreach (int del in deleted)
            {
                int[] outlinesDeleted = FindOutlines(Parts[del], positions[index]);

                if (IsInside(outlines, outlinesDeleted)) return true;
                else if (IsInside(outlinesDeleted, outlines)) return true;
                else if (CrossOutlines(outlines, outlinesDeleted)) return true;

                index++;
            }
            return false;
        }




        // Check if Point p is between outline q.
        private bool IsPointInside(int p1, int p2, int q1, int q2, int q3, int q4)
        {
            if (q1 < p1 &&
                q2 > p1 &&
                q3 < p2 &&
                q4 > p2)
                    return true;
            return false;
        }



        // Run IsPointInside for all corners of a part.
        private bool IsInside(int[] outlines, int[] outlines2)
        {
            if (IsPointInside(outlines[0], outlines[2], outlines2[0], outlines2[1], outlines2[2], outlines2[3])) return true;
            else if (IsPointInside(outlines[0], outlines[3], outlines2[0], outlines2[1], outlines2[2], outlines2[3])) return true;
            else if (IsPointInside(outlines[1], outlines[2], outlines2[0], outlines2[1], outlines2[2], outlines2[3])) return true;
            else if (IsPointInside(outlines[1], outlines[3], outlines2[0], outlines2[1], outlines2[2], outlines2[3])) return true;
            return false;
            
        }




        private bool CrossOutline(int p1, int p2, int p3, int p4, int q1, int q2, int q3, int q4)
        {
            if (p1 <= q1 &&
                p2 >= q2 &&
                p3 >= q3 &&
                p4 <= q4)
            {
                return true;
            }
            else if (p1 == q1 &&
                     p2 == q2 &&
                     ((p3 > q3 &&
                     p3 < q4) ||
                     (p4 > q3 &&
                     p4 < q4)))
            {
                return true;
            }
            else if (p3 == q3 &&
                     p4 == q4 &&
                     ((p1 > q1 &&
                     p1 < q2) ||
                     (p2 > q1 &&
                     p2 < q2)))
            {
                return true;
            }
            return false;
        }



        private bool CrossOutlines(int[] outlines, int[] outlines2)
        {
            if (CrossOutline(outlines[0], outlines[1], outlines[2], outlines[3],
                             outlines2[0], outlines2[1], outlines2[2], outlines2[3]))
                return true;
            else if (CrossOutline(outlines[2], outlines[3], outlines[0], outlines[1],
                            outlines2[2], outlines2[3], outlines2[0], outlines2[1]))
                return true;
            else if (CrossOutline(outlines2[0], outlines2[1], outlines2[2], outlines2[3],
                             outlines[0], outlines[1], outlines[2], outlines[3]))
                return true;
            else if (CrossOutline(outlines2[2], outlines2[3], outlines2[0], outlines2[1],
                            outlines[2], outlines[3], outlines[0], outlines[1]))
                return true;
            return false;
        }



        // The next methods find the best board of the boards found with Recursion()
        private void FindBest()
        {
            int i = 0;
            List<int[]> free = new List<int[]>();
            foreach (List<int> deleted in FullDeleted)
            {
                free.Add(FindFree(deleted, FullPositions[i]));
                i++;
            }

            free = BestFreeSpace(free);
            free = BestRestSpace(free);
            BestShape(free);
        }



        // Smallest free area. Space is free;
        private List<int[]> BestFreeSpace(List<int[]> free)
        {
            List<int> BestIndex = new List<int>();
            int best = Width * Height;
            for (var i = 0; i < free.Count; i++)
            {
                if (free[i][0] < best)
                {
                    best = free[i][0];
                    BestIndex = new List<int> { i };
                }
                else if (free[i][0] == best)
                {
                    BestIndex.Add(i);
                }
            }
            var start = free.Count - 1;
            for (var i = start; i>=0; i--)
            {
                if (!BestIndex.Contains(i))
                {
                    free.RemoveAt(i);
                    FullDeleted.RemoveAt(i);
                    FullPositions.RemoveAt(i);
                }
            }
            return free;
        }



        // Biggest free area when space is not free.
        // From solutions found with BestFreeSpace.
        private List<int[]> BestRestSpace(List<int[]> free)
        {
            List<int> BestIndex = new List<int>();
            int best = -1;
            for (var i = 0; i < free.Count; i++)
            {
                if (free[i][1] > best)
                {
                    best = free[i][1];
                    BestIndex = new List<int> { i };
                }
                else if (free[i][1] == best)
                {
                    BestIndex.Add(i);
                }
            }
            var start = free.Count - 1;
            for (var i = start; i >= 0; i--)
            {
                if (!BestIndex.Contains(i))
                {
                    free.RemoveAt(i);
                    FullDeleted.RemoveAt(i);
                    FullPositions.RemoveAt(i);
                }
            }
            return free;
        }



        private List<int[]> BestShape(List<int[]> free)
        {
            List<int> BestIndex = new List<int>();
            int best = -1;
            for (var i = 0; i < free.Count; i++)
            {
                if (free[i][2] > best)
                {
                    best = free[i][2];
                    BestIndex = new List<int> { i };
                }
                else if (free[i][2] == best)
                {
                    BestIndex.Add(i);
                }
            }
            var start = free.Count - 1;
            for (var i = start; i >= 0; i--)
            {
                if (!BestIndex.Contains(i))
                {
                    free.RemoveAt(i);
                    FullDeleted.RemoveAt(i);
                    FullPositions.RemoveAt(i);
                }
            }
            return free;
        }




        // Calculate the three different free-values.
        private int[] FindFree(List<int> deleted, List<int[]> positions)
        {
            int[] free = {Width * Height, Width * Height, Width * Height };
            var index2 = 0;
            foreach (int del in deleted)
            {
                int width = (positions[index2][2] == 0) ? Parts[del].Width : Parts[del].Height;
                int height = (positions[index2][2] == 0) ? Parts[del].Height : Parts[del].Width;

                free[0] -= width * height;


                if (positions[index2][0] + width + Space < Width)
                    width += Space;
                else if (positions[index2][0] + width != Width)
                    width += Width - (positions[index2][0] + width);


                if (positions[index2][1] + height + Space < Height)
                    height += Space;
                else if (positions[index2][1] + height != Height)
                    height += Height - (positions[index2][1] + height);

                free[1] -= width * height;




                bool horizontalTrue = false;
                bool verticalTrue = false;
                if (positions[index2][0] + width + 1 < Width)
                {
                    width++;
                    horizontalTrue = true;
                }

                if (positions[index2][1] + height + 1 < Height)
                {
                    height++;
                    verticalTrue = true;
                }

                free[2] -= (width * height);


                if (verticalTrue)
                {
                    for (int i = positions[index2][0]; i <= positions[index2][0] + width; i++)
                    {
                        var index = 0;
                        foreach (int d in deleted)
                        {
                            int[] outlines = FindOutlines(Parts[d], positions[index]);

                            if (IsPointInside(i, positions[index2][1] + height, outlines[0], outlines[1] + 1, outlines[2], outlines[3] + 1))
                            {
                                free[2]++;
                                break;
                            }
                            index++;
                        }
                    }
                }

                if (horizontalTrue)
                {
                    for (int i = positions[index2][1]; i <= positions[index2][1] + height; i++)
                    {
                        var index = 0;
                        
                        foreach (int d in deleted)
                        {
                            int[] outlines = FindOutlines(Parts[d], positions[index]);

                            if (IsPointInside(positions[index2][0] + width , i, outlines[0], outlines[1] + 1, outlines[2], outlines[3] + 1))
                            {
                                free[2]++;
                                break;
                            }
                            index++;
                        }
                    }
                }
                index2++;
            }

            if (free[2] < 0) free[2] = 0;
            return free;
        }


        // Move through all (free) positions and set parts where possible.
        // This method is used for "Simple Mode"
        private void CloseHoles(int index)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {   
                    // If position is free try to set part.
                    if (!IsInside(x, y, index))
                    {
                        var part_counter = 0;
                        foreach (Part part in Parts)
                        {
                            if (!FullDeleted[index].Contains(part_counter))
                            {
                                bool found = SetPart(part, FullDeleted[index], FullPositions[index], x, y, true);
                                if (found)
                                {
                                    FullDeleted[index] = AddDeleted(FullDeleted[index], part_counter);
                                    FullPositions[index] = AddPosition(FullPositions[index], x, y, 0);
                                    x += part.Width - 1;
                                }
                                else
                                {
                                    found = SetPart(part, FullDeleted[index], FullPositions[index], x, y, false);
                                    if (found)
                                    {
                                        FullDeleted[index] = AddDeleted(FullDeleted[index], part_counter);
                                        FullPositions[index] = AddPosition(FullPositions[index], x, y, 1);
                                        x += part.Height - 1;
                                    }
                                }
                            }
                            part_counter++;
                        }
                    }
                    if (Parts.Count == 0)  return;
                }
            }
        }



        // Check if position is free.
        private bool IsInside(int x, int y, int index)
        {
            var i = 0;
            foreach (int d in FullDeleted[index])
            {
                int[] outlines = FindOutlines(Parts[d], FullPositions[index][i]);

                if (IsPointInside(x, y, outlines[0], outlines[1], outlines[2], outlines[3]))
                {
                    x = outlines[1] - 1;
                    return true;
                }
                i++;
            }
            return false;
        }

    }
}
