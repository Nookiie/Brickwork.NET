using Brickwork.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Brickwork
{
    public class BrickLogic
    {
        #region Configuration Constants

        #region Volatile (Application will not work properly if changed)

        private static readonly int MODULE_HEIGHT = 2;
        private static readonly int MODULE_WIDTH = 4;

        #endregion

        #region Editable

        private static readonly int MAX_BUILDING_WIDTH_AND_HEIGHT_VALUE = 100;
        private static readonly string NOT_VALID_BUILDING_STRING = "Building is not valid!";
        private static readonly string NOT_VALID_INPUT_STRING = "Input is not valid!";

        #endregion

        #endregion

        public void Initialize()
        {
            var building = new Building();
            var readFile = true;

            while (true)
            {
                if (!GenerateInput(building, readFile) || !CheckBricks(building))
                {
                    Console.WriteLine("Resetting...\n");
                    continue;
                }

                GenerateOutput(building);

                if (readFile)
                {
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Loads Output, which is triggered after <see cref="GenerateInput(Building)"/> based on exercise instructions<br></br>
        /// Contains the main logic component for this exercises, which revolves around performing transformations on pre-selected templates<br></br>
        /// </summary>
        private void GenerateOutput(Building building)
        {
            var newArray = new int[MODULE_HEIGHT, MODULE_WIDTH];
            var isPreliminaryTransformed = false;
            int insertionVerticalBrickX = 0;

            for (var i = 0; i < building.Height; i += MODULE_HEIGHT)
            {
                for (var j = 0; j < building.Width - 1; j += MODULE_WIDTH)
                {
                    // Skip until the end of the placement where the new Array was received
                    // Once a preliminary transformation has been detected in the previous loop, the horizontal iteration will skip ahead of 
                    // the detected horizontal position of the next vertical brick

                    //    {insertionVerticalBrickX}
                    // |--...|      |--|...{insertionVerticalBrickX + 1}
                    //          ->  
                    // |--...|      |--|...{insertionVerticalBrickX + 1}
                    if (j < building.Width - 2 && isPreliminaryTransformed)
                    {
                        j = insertionVerticalBrickX + 1;
                        isPreliminaryTransformed = false;

                        if (j >= building.Width - 1)
                        {
                            break;
                        }
                    }

                    // End Minimal Transformations are done, in case of Width being a number that can not be divided by 4
                    // For instance, let's say we have the following building (2x6):
                    // 1 1 2 2 5 5
                    // 3 3 4 4 6 6

                    // The Building will then cut it down into 2 modules
                    // 1 1 2 2 | 5 5
                    // 3 3 4 4 | 6 6

                    // However, because the modules are only 2x4, proceeding with the next block (5 5 | 6 6) 
                    // will cause an IndexOutOfRange Exception in the Array
                    // Hence we have to execute End Minimal Transformations that solve this issue, where only the 2 final bricks will be transformed 
                    // and the iteration will be force skipped

                    #region Replacement Transformations

                    #region Minimal Transformations

                    // End Minimal Transformation #1 (for width % 4 != 0 only): 2 Bricks Vertical -> 2 Bricks Horizontal
                    // --      ||
                    //     -> 
                    // --      ||

                    if (building.Values[i, j] == building.Values[i, j + 1] &&
                        (building.Values[i + 1, j] != building.Values[i, j] &&
                        building.Values[i + 1, j + 1] == building.Values[i + 1, j]) &&
                        (j == 0 || j == building.Width - 2))
                    {
                        //if (j == 0)
                        //{
                        //    if (building.Values[i, j + 2] != building.Values[i, j + 3])
                        //    {
                        //        building.Values[i + 1, j] = building.Values[i, j];
                        //        building.Values[i, j + 1] = building.Values[i + 1, j + 1];

                        //        j = -2;
                        //        continue;
                        //    }
                        //}

                        if (j == building.Width - 2)
                        {
                            building.Values[i + 1, j] = building.Values[i, j];
                            building.Values[i, j + 1] = building.Values[i + 1, j + 1];

                            break;
                        }
                    }

                    // End Minimal Transformation #2 (for width % 4 != 0 only): 2 Bricks Vertical -> 2 Bricks Horizontal
                    // ||      --
                    //     -> 
                    // ||      --

                    else if (building.Values[i, j] == building.Values[i + 1, j] &&
                        building.Values[i, j + 1] != building.Values[i, j] &&
                        building.Values[i, j + 1] == building.Values[i + 1, j + 1] &&
                        (j == 0 || j == building.Width - 2))
                    {
                        //if (j == 0)
                        //{
                        //    if (building.Values[i, j + 2] != building.Values[i, j + 3])
                        //    {
                        //        building.Values[i, j + 1] = building.Values[i, j];
                        //        building.Values[i + 1, j] = building.Values[i + 1, j + 1];

                        //        j = -2;
                        //        continue;
                        //    }
                        //}

                        if (j == building.Width - 2)
                        {
                            building.Values[i, j + 1] = building.Values[i, j];
                            building.Values[i + 1, j] = building.Values[i + 1, j + 1];

                            break;
                        }
                    }

                    #endregion

                    // Replacement Transformations are done with certain templates, where dynamic operations have to be made
                    // For instance the insertion of a vertical brick at a specified index, so that it can match one of the main transformations
                    // then continue on after the inserted vertical brick

                    // Replacement Transformation #1: Left Brick Vertical, Right Brick Far Away Vertical, 2 Bricks Horizontal -> Template Transformation #2
                    // |--...|      |--|    | 1 2 2...4         1 2 2 4   
                    //          ->          |              ->
                    // |--...|      |--|    | 1 3 3...4         1 3 3 4

                    if (building.Values[i, j] != building.Values[i, j + 1] &&
                        (building.Values[i + 1, j] == building.Values[i, j] &&
                        building.Values[i, j + 1] == building.Values[i, j + 2]) &&
                        building.Values[i + 1, j + 1] == building.Values[i + 1, j + 2] &&
                        building.Values[i, j + 3] != building.Values[i + 1, j + 3])
                    {
                        for (var g = 3; g < building.Width; g++)
                        {
                            if (building.Values[i, g] == building.Values[i + 1, g])
                            {
                                insertionVerticalBrickX = g;

                                building.Values[i, j + 3] = building.Values[i, g];
                                building.Values[i + 1, j + 3] = building.Values[i, g];

                                // Compatibility Fixes in case of {even number} x {number that can not divide by 4, like 6 or 10}
                                if (building.Width % 4 != 0)
                                {
                                    building.Values[i, g] = building.Values[i, g - 1];
                                    building.Values[i + 1, g] = building.Values[i + 1, g - 1];
                                }

                                break;
                            }
                        }

                        for (var g = 4; g < building.Width - 2; g += 3)
                        {
                            if (g == 4)
                            {
                                building.Values[i, g + 1] = building.Values[i, g];
                                building.Values[i + 1, g + 1] = building.Values[i + 1, g];

                                building.Values[i, g + 3] = building.Values[i, g + 2];
                                building.Values[i + 1, g + 3] = building.Values[i + 1, g + 2];
                            }
                            else
                            {
                                building.Values[i, g + 1] = building.Values[i, g];
                                building.Values[i + 1, g + 1] = building.Values[i + 1, g];

                                building.Values[i, g + 3] = building.Values[i, g + 2];
                                building.Values[i + 1, g + 3] = building.Values[i + 1, g + 2];
                            }
                        }

                        isPreliminaryTransformed = true;
                    }


                    // Replacement Transformation #2 Left Brick Vertical Far Away, Right Brick Vertical, 2 Bricks Horizontal -> Template Transformation #2
                    // --|...|      |--|    | 1 1 3...4      4 1 1 3
                    //          ->          |            ->             
                    // --|...|      |--|    | 2 2 3...4      4 2 2 3

                    else if (building.Values[i, j] != building.Values[i + 1, j] &&
                        (building.Values[i, j] == building.Values[i, j + 1] &&
                        building.Values[i, j + 1] != building.Values[i, j + 2]) &&
                        building.Values[i + 1, j + 2] == building.Values[i, j + 2] &&
                        building.Values[i, j + 2] != building.Values[i, j + 3] &&
                        building.Values[i, j + 3] != building.Values[i + 1, j + 3])
                    {
                        for (var g = 3; g < building.Width; g++)
                        {
                            if (building.Values[i, g] == building.Values[i + 1, g])
                            {
                                insertionVerticalBrickX = g;

                                building.Values[i, j] = building.Values[i, g];
                                building.Values[i + 1, j] = building.Values[i, g];
                                break;
                            }
                        }

                        for (var g = 1; g < building.Width - 2; g += 3)
                        {
                            if (g == 1)
                            {
                                building.Values[i, g + 2] = building.Values[i, g + 1];
                                building.Values[i + 1, g + 2] = building.Values[i + 1, g + 1];

                                building.Values[i, g + 1] = building.Values[i, g];
                                building.Values[i + 1, g + 1] = building.Values[i + 1, g];
                            }
                            else
                            {
                                building.Values[i, g + 1] = building.Values[i, g];
                                building.Values[i + 1, g + 1] = building.Values[i + 1, g];

                                building.Values[i, g + 3] = building.Values[i, g + 2];
                                building.Values[i + 1, g + 3] = building.Values[i + 1, g + 2];
                            }
                        }

                        isPreliminaryTransformed = true;
                        continue;
                    }

                    #endregion

                    // Main Transformation Templates that begin transforming the array, using a module that iterates by 2x4
                    // For instance let's consider the following building (2x8):
                    // 1 1 2 2 3 3 4 4 
                    // 5 5 6 6 7 7 8 8

                    // The Module will iterate 2 times and the building will be cut into the following parts:
                    // 1 1 2 2 | 3 3 4 4 
                    // 5 5 6 6 | 7 7 8 8

                    // After which the module will look for the templates below to properly transform the numbers into the exercise criteria
                    // At this time of coding the module should be 2 x 4 (Height x Width)
                    #region Main Transformations

                    // Transformation #1: 4 Bricks Horizontal -> 2 Bricks Vertical, 2 Bricks Horizontal
                    // ----       |--|  | 1 1 2 2    1 2 2 4
                    //       ->         |         ->
                    // ----       |--|  | 3 3 4 4    1 3 3 4

                    if (building.Values[i, j] == building.Values[i, j + 1] &&
                        building.Values[i, j + 2] == building.Values[i, j + 3] &&
                        building.Values[i, j + 2] == building.Values[i, j + 3] &&
                        building.Values[i + 1, j] == building.Values[i + 1, j + 1] &&
                        building.Values[i + 1, j + 2] == building.Values[i + 1, j + 3])
                    {
                        // 2
                        newArray[0, 0] = building.Values[i, j + 2];
                        newArray[1, 0] = building.Values[i, j + 2];

                        // 1
                        newArray[0, 1] = building.Values[i, j];
                        newArray[0, 2] = building.Values[i, j];

                        // 3
                        newArray[1, 1] = building.Values[i + 1, j];
                        newArray[1, 2] = building.Values[i + 1, j];

                        // 4
                        newArray[0, 3] = building.Values[i + 1, j + 2];
                        newArray[1, 3] = building.Values[i + 1, j + 2];

                        CopyModuleOfAnArray(building.Values, newArray, i, j);
                    }

                    // Transformation #2: 4 Bricks Vertical -> 4 Bricks Horizontal
                    // ||||     ---- | 1 2 3 4      1 1 3 3
                    //      ->       |          ->
                    // ||||     ---- | 1 2 3 4      2 2 4 4

                    else if (building.Values[i, j] == building.Values[i + 1, j] &&
                         building.Values[i, j + 1] == building.Values[i + 1, j + 1] &&
                         building.Values[i, j + 2] == building.Values[i + 1, j + 2] &&
                         building.Values[i, j + 3] == building.Values[i + 1, j + 3] &&
                         building.Values[i, j] != building.Values[i, j + 1] &&
                         building.Values[i, j + 2] != building.Values[i, j + 3])
                    {
                        // 1
                        newArray[0, 0] = building.Values[i + 1, j];
                        newArray[0, 1] = building.Values[i + 1, j];

                        // 2
                        newArray[1, 0] = building.Values[i, j + 1];
                        newArray[1, 1] = building.Values[i, j + 1];

                        // 3
                        newArray[0, 2] = building.Values[i + 1, j + 2];
                        newArray[0, 3] = building.Values[i + 1, j + 2];

                        // 4
                        newArray[1, 2] = building.Values[i, j + 3];
                        newArray[1, 3] = building.Values[i, j + 3];

                        CopyModuleOfAnArray(building.Values, newArray, i, j);
                    }


                    // Transformation #3: 2 Bricks Vertical, 2 Bricks Horizontal -> 4 Bricks Horizontal 
                    // |--|       ----  | 1 2 2 4      1 1 2 2
                    //       ->         |          ->
                    // |--|       ----  | 1 3 3 4      3 3 4 4

                    else if (building.Values[i, j] == building.Values[i + 1, j] &&
                        building.Values[i, j + 1] != building.Values[i + 1, j + 1] &&
                        building.Values[i, j + 2] != building.Values[i + 1, j + 1] &&
                        building.Values[i, j + 1] == building.Values[i, j + 2] &&
                        building.Values[i, j + 3] == building.Values[i + 1, j + 3] &&
                        building.Values[i, j + 2] != building.Values[i + 1, j + 2])
                    {
                        // 1
                        newArray[0, 0] = building.Values[i, j + 1];
                        newArray[0, 1] = building.Values[i, j + 1];

                        // 2
                        newArray[0, 2] = building.Values[i, j];
                        newArray[0, 3] = building.Values[i, j];

                        // 3
                        newArray[1, 0] = building.Values[i + 1, j + 1];
                        newArray[1, 1] = building.Values[i + 1, j + 1];

                        // 4
                        newArray[1, 2] = building.Values[i, j + 3];
                        newArray[1, 3] = building.Values[i, j + 3];

                        CopyModuleOfAnArray(building.Values, newArray, i, j);
                    }


                    // Transformation #4: 4 Bricks Vertical -> 4 Bricks Horizontal
                    // ||--     --||    | 1 2 3 3       1 1 3 4
                    //      ->          |           ->
                    // ||--     --||    | 1 2 4 4       2 2 3 4
                    if (building.Values[i, j] == building.Values[i + 1, j] &&
                         building.Values[i, j + 1] == building.Values[i + 1, j + 1] &&
                         building.Values[i, j + 2] != building.Values[i + 1, j + 2] &&
                         building.Values[i, j + 3] != building.Values[i + 1, j + 3] &&
                         building.Values[i, j] != building.Values[i, j + 2] &&
                         building.Values[i, j] != building.Values[i, j + 3])
                    {
                        // 1
                        newArray[0, 0] = building.Values[i, j];
                        newArray[0, 1] = building.Values[i, j];

                        // 2
                        newArray[1, 0] = building.Values[i, j + 1];
                        newArray[1, 1] = building.Values[i, j + 1];

                        // 3
                        newArray[0, 2] = building.Values[i, j + 2];
                        newArray[1, 2] = building.Values[i, j + 2];

                        // 3
                        newArray[0, 3] = building.Values[i + 1, j + 2];
                        newArray[1, 3] = building.Values[i + 1, j + 2];

                        CopyModuleOfAnArray(building.Values, newArray, i, j);
                    }

                    // Transformation #5: 4 Bricks Vertical -> 4 Bricks Horizontal
                    // --||     ||--    | 1 1 3 4       1 2 3 3
                    //      ->          |           ->
                    // --||     ||--    | 2 2 3 4       1 2 4 4
                    else if (building.Values[i, j] != building.Values[i + 1, j] &&
                         building.Values[i, j + 1] != building.Values[i + 1, j + 1] &&
                         building.Values[i, j + 2] == building.Values[i + 1, j + 2] &&
                         building.Values[i, j + 3] == building.Values[i + 1, j + 3] &&
                         building.Values[i, j] != building.Values[i, j + 2] &&
                         building.Values[i, j] != building.Values[i, j + 3])
                    {
                        // 1
                        newArray[0, 0] = building.Values[i, j];
                        newArray[1, 0] = building.Values[i, j];

                        // 2
                        newArray[0, 1] = building.Values[i + 1, j];
                        newArray[1, 1] = building.Values[i + 1, j];

                        // 3
                        newArray[0, 2] = building.Values[i, j + 2];
                        newArray[0, 3] = building.Values[i, j + 2];

                        // 4
                        newArray[1, 2] = building.Values[i + 1, j + 3];
                        newArray[1, 3] = building.Values[i + 1, j + 3];

                        CopyModuleOfAnArray(building.Values, newArray, i, j);
                    }

                    #endregion
                }
            }

            PrintArray(building);
        }

        /// <summary>
        /// Loads input data from the console, per the exercise example guide <br></br>
        /// This includes width and height and brick layers as per exercise example guide<br></br>
        /// </summary>
        /// <param name="building">Brick to Load Data Into</param>
        /// <param name="mustReadFile">Boolean whether or not to use the file parameter read functionality</param>
        private bool GenerateInput(Building building, bool mustReadFile = false)
        {
            // Automatic Reading from file, will read an input.txt file in the working directory and execute
            // Used for quick testing and debugging

            #region File Read

            if (mustReadFile)
            {
                var inputTextFileLocation = @"../../../input.txt";
                var input = File.ReadAllLines(inputTextFileLocation);

                var buildingParametersText = input[0]
                    .Split(' ');

                var buildingNumbersFile = new Collection<int>();

                foreach (var numberString in buildingParametersText)
                {
                    if (!int.TryParse(numberString, out int value))
                    {
                        Console.WriteLine(NOT_VALID_INPUT_STRING);
                        return false;
                    }

                    buildingNumbersFile.Add(value);
                }

                building.Height = buildingNumbersFile[0];
                building.Width = buildingNumbersFile[1];

                if (!IsBuildingValid(building))
                {
                    return false;
                }

                building.Values = new int[building.Height, building.Width];

                for (int i = 1; i < building.Height + 1; i++)
                {
                    var numbersString = input[i].Split(' ');

                    for (int j = 0; j < building.Width; j++)
                    {
                        if (!int.TryParse(numbersString[j], out int value))
                        {
                            Console.WriteLine(NOT_VALID_INPUT_STRING);
                            return false;
                        }

                        building.Values[i - 1, j] = value;
                    }
                }

                return true;
            }
            #endregion

            #region Console Input

            else
            {

                var buildingParameters = Console.ReadLine()
                    .Split(' ');

                var buildingNumbers = new Collection<int>();

                if (buildingParameters.Length != 2)
                {
                    Console.WriteLine(NOT_VALID_INPUT_STRING);
                    return false;
                }

                foreach (var numberString in buildingParameters)
                {
                    if (!int.TryParse(numberString, out int number))
                    {
                        Console.WriteLine(NOT_VALID_INPUT_STRING);
                        return false;
                    }

                    buildingNumbers.Add(number);
                }

                building.Height = buildingNumbers[0];
                building.Width = buildingNumbers[1];

                if (!IsBuildingValid(building))
                {
                    return false;
                }

                building.Values = new int[building.Height, building.Width];
                for (int i = 0; i < building.Height; i++)
                {
                    var valueString = Console.ReadLine()
                        .Split(' ');

                    for (int j = 0; j < building.Width; j++)
                    {
                        if (valueString.Length != building.Width)
                        {
                            Console.WriteLine(NOT_VALID_INPUT_STRING);
                            return false;
                        }

                        if (!int.TryParse(valueString[j], out int value))
                        {
                            Console.WriteLine(NOT_VALID_INPUT_STRING);
                            return false;
                        }

                        building.Values[i, j] = value;
                    }
                }

                return true;
            }


            #endregion
        }

        /// <summary>
        /// Determines whether all bricks are valid and can be executed through the method <see cref="GenerateOutput(Building)"/>.<br></br>
        /// Checks include whether the brick's parameters are valid, according to the exercise.<br></br>
        /// Also provides details for every <see cref="Brick"/> in the <see cref="Building"/>
        /// </summary>
        /// <param name="building">Brick</param>
        /// <returns></returns>
        private bool CheckBricks(Building building, bool mustGenerateBrickEntities = true)
        {
            for (var i = 0; i < building.Height; i += 2)
            {
                for (var j = 0; j < building.Width; j++)
                {
                    if (building.Values[i, j] == building.Values[i + 1, j])
                    {
                        if (mustGenerateBrickEntities)
                        {
                            building.Bricks.Add(new Brick(i, j, i + 1, j, building.Values[i, j], BrickType.Vertical));
                        }
                    }
                    else if (building.Values[i, j] == building.Values[i, j + 1])
                    {
                        if (mustGenerateBrickEntities)
                        {
                            building.Bricks.Add(new Brick(i, j, i, j + 1, building.Values[i, j], BrickType.Horizontal));

                            if (building.Values[i + 1, j] == building.Values[i + 1, j + 1])
                            {
                                building.Bricks.Add(new Brick(i + 1, j, i + 1, j + 1, building.Values[i + 1, j], BrickType.Horizontal));
                            }
                        }

                        j++;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid Brick Values on Coordinates:{i} {j}");
                        return false;
                    }
                }
            }

            if (!IsBuildingBricksValid(building))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// A series of error checks, regarding the validity of <see cref="Building"/><br></br>
        /// If an error is detected, the application will reset
        /// </summary>
        /// <param name="building">Building</param>
        /// <returns></returns>
        private bool IsBuildingValid(Building building)
        {
            if (building.Width % 2 != 0 || building.Height % 2 != 0)
            {
                Console.WriteLine($"{NOT_VALID_BUILDING_STRING}\nLayer width or height are not even!");
                return false;
            }

            if (building.Height >= MAX_BUILDING_WIDTH_AND_HEIGHT_VALUE || building.Width >= MAX_BUILDING_WIDTH_AND_HEIGHT_VALUE)
            {
                Console.WriteLine($"{NOT_VALID_BUILDING_STRING}!\nLayer width or height must be less than 100!");
                return false;
            }

            if (building.Width < building.Height)
            {
                Console.WriteLine("-1");
                return false;
            }

            return true;
        }

        /// <summary>
        /// A specialized error check, performing a check on where all bricks are valid<br></br>
        /// A building brick must have 2 equal numbers vertically or horizontally
        /// </summary>
        /// <param name="building">Building</param>
        /// <returns>If building bricks are valid - true, else - false</returns>
        private bool IsBuildingBricksValid(Building building)
        {
            foreach (var value in building.Values)
            {
                if (value > building.MaxBrickValue || value <= 0)
                {
                    Console.WriteLine($"{NOT_VALID_BUILDING_STRING}!\nA value in the building is incorrect: {value}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Copies all values of the current module iteration into the new building values
        /// </summary>
        /// <param name="sourceArray">Source Array (newArray)</param>
        /// <param name="destinationArray">Destination Array (building.Values)</param>
        /// <param name="currentHeight">Current Height (i)</param>
        /// <param name="currentWidth">Current Width(j)</param>
        private void CopyModuleOfAnArray(int[,] sourceArray, int[,] destinationArray, int currentHeight, int currentWidth)
        {
            for (var v = 0; v < MODULE_HEIGHT; v++)
            {
                for (var s = 0; s < MODULE_WIDTH; s++)
                {
                    sourceArray[currentHeight + v, currentWidth + s] = destinationArray[v, s];
                }
            }
        }

        private void PrintArray(Building building)
        {
            for (var i = 0; i < building.Width; i++)
            {
                Console.Write("--");
            }

            Console.Write("\n");
            for (var i = 0; i < building.Height; i++)
            {
                for (var j = 0; j < building.Width; j++)
                {
                    Console.Write($"{building.Values[i, j]} ");
                }
                Console.Write("\n");
            }

            Console.Write("\n");
            for (var i = 0; i < building.Width; i++)
            {
                Console.Write("**");
            }
            Console.Write("\n\n");
        }
    }
}
