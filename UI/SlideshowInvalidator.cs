using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.UI
{
    public class SlideshowInvalidator : IInvalidator
    {
        private readonly IInvalidator innerInvalidator;
        private readonly Action<float, float, float, float> invalidationAction;

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

        public SlideshowInvalidator(IInvalidator innerInvalidator, Action<float, float, float, float> invalidationAction)
        {
            this.innerInvalidator = innerInvalidator;
            this.invalidationAction = invalidationAction;
        }

        public void Invalidate(float x, float y, float width, float height)
        {
            invalidationAction?.Invoke(x, y, width, height);
        }
    }
}
