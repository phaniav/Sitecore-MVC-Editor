using System;
using System.IO;
using System.Linq;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Mvc.ExperienceEditor.Presentation;
using Sitecore.Mvc.Pipelines.Response.RenderRendering;

namespace SampleSite.MvcEditor {
	public class AddEditorRenderingWrapper : RenderRenderingProcessor {
		public override void Process(RenderRenderingArgs args) {
			if (args.Rendered || Context.Site == null || !Context.PageMode.IsExperienceEditorEditing || args.Rendering.RenderingType == "Layout") {
				return;
			}

			IMarker marker = GetMarker(args);

			if (marker == null) {
				return;
			}

			args.Disposables.Add(new EditorRenderingWrapper(args.Writer, marker));
		}

		private IMarker GetMarker(RenderRenderingArgs args) {
			IMarker marker = null;
			RenderingItem renderingItem = args.Rendering.RenderingItem;

			if (renderingItem != null) {
				marker = new EditorComponentRenderingMarker(renderingItem.Name);
			}

			return marker;
		}
	}

	public class EndEditorRenderingWrapper : RenderRenderingProcessor {
		public override void Process(RenderRenderingArgs args) {
			foreach (IDisposable wrapper in args.Disposables.OfType<EditorRenderingWrapper>()) {
				wrapper.Dispose();
			}
		}
	}

	public class EditorRenderingWrapper : Wrapper {
		public EditorRenderingWrapper(TextWriter writer, IMarker marker) : base(writer, marker) {
		}
	}

	public class EditorComponentRenderingMarker : IMarker {
		private readonly string _componentName;

		public EditorComponentRenderingMarker(string componentName) {
			_componentName = componentName;
		}

		public string GetStart() {
			return $"<div class=\"component-wrapper scRendering {_componentName.Replace(" ", string.Empty)}\"><span class=\"wrapper-header\">{_componentName}</span><div class=\"component-content clearfix\">";
		}

		public string GetEnd() {
			return "</div></div>";
		}
	}
}