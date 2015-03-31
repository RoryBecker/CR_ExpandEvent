using System.ComponentModel.Composition;
using DevExpress.CodeRush.Common;

namespace CR_ExpandEvent
{
	[Export(typeof(IVsixPluginExtension))]
	public class CR_ExpandEventExtension : IVsixPluginExtension { }
}