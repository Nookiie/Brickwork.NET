using Brickwork.Entities;
using System.Collections.Generic;

namespace Brickwork
{
    /// <summary>
    /// The overall <b>Brick</b> that is composed of multiple objects of type <see cref="Entities.Brick"/>
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Building's Values - includes all values in the form of a matrix<br></br>
        /// The values of the 2D matrix are coordinates (Y - height and X - width, respectively)
        /// </summary>
        public int[,] Values { get; set; }

        public IList<Brick> Bricks { get; set; } = new List<Brick>();

        /// <summary>
        /// The Height of the Brick - Usually this is the first number of the input
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The Width of The Brick - Usually this is the second number of the input
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The Max Number, that is decided by the <see cref="Height"/> and <see cref="Width"/> for the Brick Placement and must be equal to the Brick Count.<br></br>
        /// Any higher value can not be a part of the building
        /// </summary>
        public int MaxBrickValue
        {
            get => Height * Width / 2;
        }
    }
}
