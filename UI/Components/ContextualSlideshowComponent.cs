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
        private class SlideshowComponent
        {
            public IComponent Component { get; }
            public TimeStamp LastDequeue { get; set; }

            public SlideshowComponent(IComponent component)
            {
                Component = component;
                LastDequeue = TimeStamp.Now;
            }
        }

        private LiveSplitState state;
        private IList<SlideshowComponent> slideshowComponents;
        private Queue<IComponent> queuedComponents;
        private TimeStamp lastSwap;
        private TimeStamp lastInvalidation;

        public string ComponentName
            => "Contextual Slideshow";

        public IDictionary<string, Action> ContextMenuControls
            => null;

        public float HorizontalWidth
            => slideshowComponents.Max(x => x.Component.HorizontalWidth);

        public float VerticalHeight
            => slideshowComponents.Max(x => x.Component.VerticalHeight);

        public float MinimumHeight
            => slideshowComponents.Max(x => x.Component.MinimumHeight);

        public float MinimumWidth
            => slideshowComponents.Max(x => x.Component.MinimumWidth);

        public float PaddingBottom
            => slideshowComponents.Min(x => x.Component.PaddingBottom);

        public float PaddingLeft
            => slideshowComponents.Min(x => x.Component.PaddingLeft);

        public float PaddingRight
            => slideshowComponents.Min(x => x.Component.PaddingRight);

        public float PaddingTop
            => slideshowComponents.Min(x => x.Component.PaddingTop);

        public ContextualSlideshowComponent(LiveSplitState state)
        {
            this.state = state;
            slideshowComponents = new List<SlideshowComponent>
            {
                new SlideshowComponent(new PossibleTimeSave(state)),
                new SlideshowComponent(new PreviousSegment(state)),
                new SlideshowComponent(new RunPrediction(state))
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
            invalidateAllComponents(invalidator, state, width, height, mode);
            possiblySwapOutComponent(invalidator, width, height);
            possiblyEnqueueComponentsThatDontInvalidate();
        }

        private void possiblyEnqueueComponentsThatDontInvalidate()
        {
            var now = TimeStamp.Now;

            if (now - (lastSwap ?? now) > TimeSpan.FromSeconds(12))
            {
                var oldestDequeue = slideshowComponents
                    .Where(x => !queuedComponents.Contains(x.Component))
                    .OrderBy(x => now - x.LastDequeue)
                    .LastOrDefault();

                if (oldestDequeue != null)
                {
                    enqueueComponent(oldestDequeue.Component);
                }
            }
        }

        private void invalidateAllComponents(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            IComponent currentComponent = null;
            var slideshowInvalidator = new SlideshowInvalidator(invalidator, (x, y, w, h) =>
            {
                invalidateComponent(invalidator, x, y, w, h, currentComponent);
            });

            foreach (var component in slideshowComponents)
            {
                currentComponent = component.Component;
                currentComponent.Update(slideshowInvalidator, state, width, height, mode);
            }
        }

        private void invalidateComponent(IInvalidator invalidator, float x, float y, float w, float h, IComponent currentComponent)
        {
            if (currentComponent != null)
            {
                enqueueComponent(currentComponent);
                if (invalidator != null && queuedComponents.FirstOrDefault() == currentComponent)
                {
                    lastInvalidation = TimeStamp.Now;
                    invalidator.Invalidate(x, y, w, h);
                }
            }
        }

        private void enqueueComponent(IComponent currentComponent)
        {
            if (!queuedComponents.Contains(currentComponent))
            {
                System.Diagnostics.Debug.WriteLine($"Enqueue { currentComponent.ComponentName }");
                if (!queuedComponents.Any())
                    lastSwap = TimeStamp.Now;

                queuedComponents.Enqueue(currentComponent);
            }
        }

        private void possiblySwapOutComponent(IInvalidator invalidator, float width, float height)
        {
            if (queuedComponents.Count > 1 &&
                (TimeStamp.Now - (lastSwap ?? TimeStamp.Now) > TimeSpan.FromSeconds(8)
                || TimeStamp.Now - (lastInvalidation ?? TimeStamp.Now) > TimeSpan.FromSeconds(3)))
            {
                lastSwap = TimeStamp.Now;
                var dequeuedComponent = queuedComponents.Dequeue();
                var slideshowComponent = slideshowComponents.FirstOrDefault(x => x.Component == dequeuedComponent);
                if (slideshowComponent != null)
                {
                    slideshowComponent.LastDequeue = lastSwap;
                }

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
                        component.Component.Dispose();
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
