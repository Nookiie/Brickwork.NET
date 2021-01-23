using System;
using System.Collections.Generic;
using System.Text;

namespace Brickwork.Entities
{
    /// <summary>
    /// A class, used solely for debugging and providing insights into how many and of which type bricks there are in a <see cref="Building"/><br></br>
    /// Decided to keep anyway to help out in any debugging endeavours
    /// </summary>
    public class Brick
    {
        public Brick()
        {

        }

        public Brick(int value1Y, int value1X, int value2Y, int value2X, int value, BrickType brickType)
        {
            Coordinates = new ValueTuple<int, int, int, int>(value1Y, value1X, value2Y, value2X);
            Value = value;
            BrickType = brickType;
        }

        public int Value { get; set; }

        /// <summary>
        /// BrickType - Can be Vertical or Horizontal, depending on the coordinates<br></br>
        /// Non-funcational, used solely to help with debugging and insights
        /// </summary>
        public BrickType BrickType { get; set; }

        /// <summary>
        /// Coordinates for the Brick alongside the Building<br></br>
        /// Format: value1Y, value1X, value2Y, value2X
        /// </summary>
        public ValueTuple<int, int, int, int> Coordinates { get; set; }
    }
}
