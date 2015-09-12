using System;
using LiveSplit.Model;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(ContextualSlideshowFactory))]

namespace LiveSplit.UI.Components
{
    public class ContextualSlideshowFactory : IComponentFactory
    {
        public ComponentCategory Category 
            => ComponentCategory.Media;

        public string ComponentName 
            => "Contextual Slideshow";

        public string Description 
            => "Creates a Slideshow of multiple Components, always showing the most relevant one.";

        public IComponent Create(LiveSplitState state)
            => new ContextualSlideshowComponent(state);

        public string UpdateName 
            => ComponentName;

        public string XMLURL
            => "http://livesplit.org/update/Components/update.LiveSplit.ContextualSlideshow.xml";

        public string UpdateURL
            => "http://livesplit.org/update/";

        public Version Version
            => Version.Parse("1.6");
    }
}
