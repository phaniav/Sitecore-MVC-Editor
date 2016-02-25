using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore;
using Sitecore.Mvc.Extensions;
using Sitecore.Mvc.Pipelines;
using Sitecore.Mvc.Pipelines.Response.RenderPlaceholder;
using Sitecore.Mvc.Pipelines.Response.RenderRendering;
using Sitecore.Mvc.Presentation;

namespace SampleSite.MvcEditor {
	public class EditorPlaceholder : RenderPlaceholderProcessor {
		public override void Process(RenderPlaceholderArgs args) {
			Render(args.PlaceholderName, args.Writer, args);
		}

		protected virtual void Render(string placeholderName, TextWriter writer, RenderPlaceholderArgs args) {
			if (Context.PageMode.IsExperienceEditorEditing) {
				writer.Write($"<div class=\"component-wrapper scPlaceholder {placeholderName.Replace(" ", string.Empty)}\"><span class=\"wrapper-header\">{placeholderName}</span><div class=\"component-content clearfix\">");
			}

			foreach (Rendering rendering in GetRenderings(placeholderName, args)) {
				PipelineService.Get().RunPipeline("mvc.renderRendering", new RenderRenderingArgs(rendering, writer));
			}

			if (Context.PageMode.IsExperienceEditorEditing) {
				writer.Write("</div></div>");
			}
		}

		protected virtual IEnumerable<Rendering> GetRenderings(string placeholderName, RenderPlaceholderArgs args) {
			string placeholderPath = PlaceholderContext.Current.ValueOrDefault(context => context.PlaceholderPath).OrEmpty();
			Guid deviceId = GetPageDeviceId(args);

			return args.PageContext.PageDefinition.Renderings.Where(r => {
				if (!(r.DeviceId == deviceId)) {
					return false;
				}

				return r.Placeholder.EqualsText(placeholderName) || r.Placeholder.EqualsText(placeholderPath);
			});
		}

		protected virtual Guid GetPageDeviceId(RenderPlaceholderArgs args) {
			Guid guid1 = args.OwnerRendering.ValueOrDefault(rendering => rendering.DeviceId);

			if (guid1 != Guid.Empty) {
				return guid1;
			}

			Guid guid2 = (PageContext.Current.PageView as RenderingView).ValueOrDefault(view => view.Rendering).ValueOrDefault(rendering => rendering.DeviceId);

			return guid2 != Guid.Empty ? guid2 : Context.Device.ID.ToGuid();
		}
	}
}