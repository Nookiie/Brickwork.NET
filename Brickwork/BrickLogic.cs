using Brickwork.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Brickwork
{
    public class BrickLogic
    {
        // Configuration Constants 
        private static readonly int MAX_BUILDING_WIDTH_AND_HEIGHT_VALUE = 100;
        private static readonly string NOT_VALID_BUILDING_STRING = "Building is not valid!";
        private static readonly string NOT_VALID_INPUT_STRING = "Input is not valid!";

        public void Initialize()
        {
            Building building = new Building();

            while (true)
            {
                if (!GenerateInput(building, true) || !GenerateBricks(building) || !IsBuildingBricksValid(building))
                {
                    Console.WriteLine("Building is not valid!\nResetting...");
                    continue;
                }

                GenerateOutput(building);
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Loads Output, which is triggered after <see cref="GenerateInput(Building)"/> based on exercise instructions<br></br>
        /// Contains the main logic component for this exercises
        /// </summary>
        public void GenerateOutput(Building building)
        {
            for (var i = 0; i < building.Height; i += 2)
            {
                for (var j = 0; j < building.Width - 1; j += 4)
                {
                    ////Preliminary Transformation
                    //// First - Hor, Second - Ver
                    //if (building.Values[i, j] != building.Values[i + 1, j] &&
                    //    (building.Values[i, j + 1] == building.Values[i, j]
                    //    && building.Values[i, j + 1] != building.Values[i, j + 2]))
                    //{
                    //    var brick = new Brick();
                    //    brick.BrickType = BrickType.Vertical;

                    //    for (int v = 3; v < building.Width; v++)
                    //    {
                    //        if (building.Values[i, v] == building.Values[i + 1, v])
                    //        {
                    //            brick.Value = building.Values[i, v];
                    //            brick.Coordinates = new ValueTuple<int, int, int, int>(i, v, i + 1, v);
                    //        }
                    //    }

                    //    var savedNumber = building.Values[i, j + 2];

                    //    building.Values[i, j] = brick.Value;
                    //    building.Values[i + 1, j] = brick.Value;

                    //    building.Values[i, j + 2] = building.Values[i, j + 1];
                    //    building.Values[i + 1, j + 2] = building.Values[i + 1, j + 1];

                    //    building.Values[i, j + 3] = savedNumber;
                    //    building.Values[i + 1, j + 3] = savedNumber;

                    //    building.Values[i,j+3]
                    //}
                    // Transformation: First Brick: Horizontal -> Vertical (4 bricks in a single iteration)
                    if (building.Values[i, j] == building.Values[i, j + 1])
                    {
                        var savedNumber = building.Values[i, j];
                        var savedNumber2 = building.Values[i, j + 3];

                        //2 ver 2
                        building.Values[i, j] = building.Values[i, j + 2];
                        building.Values[i + 1, j] = building.Values[i, j + 2];

                        //1 hor 1 ->
                        building.Values[i, j + 1] = savedNumber;
                        building.Values[i, j + 2] = savedNumber;

                        //3 hor 3 ->
                        building.Values[i + 1, j + 2] = building.Values[i + 1, j + 1];

                        //4 ver 4
                        building.Values[i, j + 3] = building.Values[i + 1, j + 3];

                        if (j != 0 && building.Values[i, j] == building.Values[i + 1, j + 1])
                        {
                            building.Values[i + 1, j + 1] = 4;
                        }
                    }
                    else
                    {
                        // Transformation: First Brick: Vertical -> Horizontal (4 bricks at a time)
                        if (building.Values[i + 1, j] == building.Values[i, j])
                        {
                            var savedNumber = building.Values[i, j];
                            var savedNumber2 = building.Values[i, j + 3];
                            var savedNumber3 = building.Values[i + 1, j + 2];

                            //5 hor 5
                            building.Values[i, j] = building.Values[i, j + 1];
                            building.Values[i, j + 1] = building.Values[i, j + 2];

                            //6 hor 6
                            building.Values[i, j + 2] = savedNumber;
                            building.Values[i, j + 3] = savedNumber;

                            //7 hor 7
                            building.Values[i + 1, j] = building.Values[i + 1, j + 1];
                            building.Values[i + 1, j + 1] = savedNumber3;

                            building.Values[i + 1, j + 2] = savedNumber2;
                            building.Values[i + 1, j + 3] = savedNumber2;

                            if (j != 0 && building.Values[i, j] == building.Values[i + 1, j + 1])
                            {
                                building.Values[i + 1, j + 1] = 4;
                            }

                        }
                    }
                }
            }

            PrintList(building);
        }

        /// <summary>
        /// Loads input data from the console, per the exercise example guide <br></br>
        /// This includes width and height and brick layers as per exercise example guide
        /// </summary>
        /// <param name="building">Brick to load data into</param>
        public bool GenerateInput(Building building, bool readFile = false)
        {
            // Automatic Reading from file, will read an input.txt file in the binaries and execute
            // Used for quick testing and debugging

            #region File Read

            if (readFile)
            {
                var input = File.ReadAllText("input.txt");
                var buildingParametersText = input.Substring(0, 3)
                    .Split(' ');

                var buildingNumbersFile = new Collection<int>();

                foreach (var numberString in buildingParametersText)
                {
                    if (!int.TryParse(numberString, out int number))
                    {
                        Console.WriteLine(NOT_VALID_INPUT_STRING);
                        return false;
                    }

                    buildingNumbersFile.Add(number);
                }

                building.Height = buildingNumbersFile[0];
                building.Width = buildingNumbersFile[1];

                if (!IsBuildingValid(building))
                {
                    return false;
                }

                building.Values = new int[building.Height, building.Width];

                var valueString = input.Substring(3)
                      .Split("\r\n")
                      .Skip(1)
                      .ToArray<string>();

                for (int i = 0; i < building.Height; i++)
                {
                    var numbersString = valueString[i].Split(' ');

                    for (int j = 0; j < building.Width; j++)
                    {
                        if (!int.TryParse(numbersString[j], out int value))
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

        /// <summary>
        /// Determines whether the brick is valid and can be executed through the method <see cref="GenerateOutput(Building)"/>.<br></br>
        /// Checks include whether the brick's parameters are valid, according to the exercise.<br></br>
        /// Also provides details for every <see cref="Brick"/> in the <see cref="Building"/>
        /// </summary>
        /// <param name="building">Brick</param>
        /// <returns></returns>
        public bool GenerateBricks(Building building)
        {
            for (var i = 0; i < building.Height; i += 2)
            {
                for (var j = 0; j < building.Width - 1; j++)
                {
                    // Scanning for the last row in the matrix for horizontal bricks
                    if (building.Height - 1 == i)
                    {
                        if (building.Values[i, j] == building.Values[i, j + 1])
                        {
                            building.Bricks.Add(new Brick(i, j, i, j + 1, building.Values[i, j], BrickType.Horizontal));
                        }
                    }
                    else
                    {
                        if (building.Values[i, j] == building.Values[i + 1, j])
                        {
                            building.Bricks.Add(new Brick(i, j, i + 1, j, building.Values[i, j], BrickType.Vertical));
                        }
                        else if (building.Values[i, j] == building.Values[i, j + 1])
                        {
                            building.Bricks.Add(new Brick(i, j, i, j + 1, building.Values[i, j], BrickType.Horizontal));
                            j++;
                        }
                        else
                        {
                            Console.WriteLine($"Invalid Brick Values on Coordinates:{i} {j}");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// A series of error checks, regarding the validity of <see cref="Building"/><br></br>
        /// If an error is detected, the application will reset
        /// </summary>
        /// <param name="building">Building</param>
        /// <returns></returns>
        public bool IsBuildingValid(Building building)
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

            return true;
        }

        public bool IsBuildingBricksValid(Building building)
        {
            foreach (var value in building.Values)
            {
                if (value > building.MaxNumber || value <= 0)
                {
                    Console.WriteLine($"{NOT_VALID_BUILDING_STRING}!\nA value in the building is incorrect: {value}");
                    return false;
                }
            }
            return true;
        }

        public void PrintList(Building building)
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
