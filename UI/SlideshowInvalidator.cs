using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.UI
{
    public delegate void Invalidation(float x, float y, float width, float height);

    public class SlideshowInvalidator : IInvalidator
    {
        private readonly IInvalidator innerInvalidator;
        private readonly Invalidation invalidationCallback;

        public Matrix Transform
        {
            get
            {
                return innerInvalidator.Transform;
            }
            set
            {
                innerInvalidator.Transform = value;
            }
        }

        public SlideshowInvalidator(IInvalidator innerInvalidator, Invalidation invalidationCallback)
        {
            this.innerInvalidator = innerInvalidator;
            this.invalidationCallback = invalidationCallback;
        }

        public void Invalidate(float x, float y, float width, float height)
        {
            invalidationCallback?.Invoke(x, y, width, height);
        }
    }
}
