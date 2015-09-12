using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;

namespace LiveSplit.UI.Components
{
    public class ContextualSlideshowComponent : IComponent
    {
        private LiveSplitState state;
        private IList<IComponent> slideshowComponents;
        private Queue<IComponent> queuedComponents;
        private TimeStamp lastSwap;
        private TimeStamp lastInvalidation;

        public string ComponentName
            => "Contextual Slideshow";

        public IDictionary<string, Action> ContextMenuControls
            => null;

        public float HorizontalWidth
            => slideshowComponents.Max(x => x.HorizontalWidth);

        public float VerticalHeight
            => slideshowComponents.Max(x => x.VerticalHeight);

        public float MinimumHeight
            => slideshowComponents.Max(x => x.MinimumHeight);

        public float MinimumWidth
            => slideshowComponents.Max(x => x.MinimumWidth);

        public float PaddingBottom
            => slideshowComponents.Min(x => x.PaddingBottom);

        public float PaddingLeft
            => slideshowComponents.Min(x => x.PaddingLeft);

        public float PaddingRight
            => slideshowComponents.Min(x => x.PaddingRight);

        public float PaddingTop
            => slideshowComponents.Min(x => x.PaddingTop);

        public ContextualSlideshowComponent(LiveSplitState state)
        {
            this.state = state;
            slideshowComponents = new List<IComponent>
            {
                new PossibleTimeSave(state),
                new PreviousSegment(state),
                new RunPrediction(state)
            };
            queuedComponents = new Queue<IComponent>();
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            var component = queuedComponents.FirstOrDefault();
            if (component != null)
            {
                component.DrawHorizontal(g, state, height, clipRegion);
            }
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            var component = queuedComponents.FirstOrDefault();
            if (component != null)
            {
                component.DrawVertical(g, state, width, clipRegion);
            }
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return document.CreateElement("Settings");
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return null;
        }

        public void SetSettings(XmlNode settings)
        {
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            IComponent currentComponent = null;
            var slideshowInvalidator = new SlideshowInvalidator(invalidator, (x, y, w, h) =>
            {
                if (currentComponent != null)
                {
                    if (!queuedComponents.Contains(currentComponent))
                    {
                        if (!queuedComponents.Any())
                            lastSwap = TimeStamp.Now;

                        queuedComponents.Enqueue(currentComponent);
                    }
                    if (invalidator != null && queuedComponents.FirstOrDefault() == currentComponent)
                    {
                        lastInvalidation = TimeStamp.Now;
                        invalidator.Invalidate(x, y, w, h);
                    }
                }
            });

            foreach (var component in slideshowComponents)
            {
                currentComponent = component;
                component.Update(slideshowInvalidator, state, width, height, mode);
            }

            if (queuedComponents.Count > 1 && 
                (  TimeStamp.Now - (lastSwap ?? TimeStamp.Now) > TimeSpan.FromSeconds(10)
                || TimeStamp.Now - (lastInvalidation ?? TimeStamp.Now) > TimeSpan.FromSeconds(3)))
            {
                lastSwap = TimeStamp.Now;
                queuedComponents.Dequeue();
                if (invalidator != null)
                {
                    lastInvalidation = TimeStamp.Now;
                    invalidator.Invalidate(0, 0, width, height);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var component in slideshowComponents)
                    {
                        component.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
