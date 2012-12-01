using System;

namespace Jmelosegui.Windows.Native
{
    [Serializable]
    public struct RECT
    {
        public readonly int Left;
        
        public readonly int Top;
        
        public readonly int Right;
        
        public readonly int Bottom;
        
        public static readonly RECT Empty;
        
        public int Width
        {
            get { return Math.Abs(Right - Left); }
        }
        
        public int Height
        {
            get { return Bottom - Top; }
        }
        
        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
        
        public RECT(RECT rcSrc)
        {
            Left = rcSrc.Left;
            Top = rcSrc.Top;
            Right = rcSrc.Right;
            Bottom = rcSrc.Bottom;
        }
        
        public bool IsEmpty
        {
            get
            {
                return Left >= Right || Top >= Bottom;
            }
        }

        /// <summary> 
        /// Return a user friendly representation of this struct 
        /// </summary>
        public override string ToString()
        {
            if (this == Empty)
            {
                return "RECT {Empty}";
            }
            return "RECT { left : " + Left + " / top : " + Top + " / right : " + Right + " / bottom : " + Bottom + " }";
        }

        /// <summary> 
        /// Determine if 2 RECT are equal (deep compare) 
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is RECT))
            {
                return false;
            }
            return (this == (RECT) obj);
        }

        /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
        public override int GetHashCode()
        {
            return Left.GetHashCode() + Top.GetHashCode() + Right.GetHashCode() + Bottom.GetHashCode();
        }


        /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
        public static bool operator ==(RECT rect1, RECT rect2)
        {
            return (rect1.Left == rect2.Left && rect1.Top == rect2.Top && rect1.Right == rect2.Right &&
                    rect1.Bottom == rect2.Bottom);
        }

        /// <summary> Determine if 2 RECT are different(deep compare)</summary>
        public static bool operator !=(RECT rect1, RECT rect2)
        {
            return !(rect1 == rect2);
        }
    }
}