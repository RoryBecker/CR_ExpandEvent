using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.CodeRush.Core;
using DevExpress.CodeRush.PlugInCore;
using DevExpress.CodeRush.StructuralParser;
using DevExpress.Refactor;
namespace CR_ExpandEvent
{
	public partial class PlugIn1 : StandardPlugIn
	{
		// DXCore-generated code...
		#region InitializePlugIn
		public override void InitializePlugIn()
		{
			base.InitializePlugIn();
			registerExpandEvent();
			registerCompressEvent();
		}
		#endregion
		#region FinalizePlugIn
		public override void FinalizePlugIn()
		{
			//
			// TODO: Add your finalization code here.
			//

			base.FinalizePlugIn();
		}
		#endregion

		private Event _EventToExpand;
		private Event _EventToCompress;
		private Variable _EventToCompressFieldDeclaration;
		public void registerExpandEvent()
		{
			DevExpress.Refactor.Core.RefactoringProvider ExpandEvent = new DevExpress.Refactor.Core.RefactoringProvider(components);
			((System.ComponentModel.ISupportInitialize)(ExpandEvent)).BeginInit();
			ExpandEvent.ProviderName = "Expand Event"; // Should be Unique
			ExpandEvent.DisplayName = "Expand Event";
			ExpandEvent.CheckAvailability += ExpandEvent_CheckAvailability;
			ExpandEvent.PreparePreview += ExpandEvent_PreparePreview;
			ExpandEvent.Apply += ExpandEvent_Execute;
			((System.ComponentModel.ISupportInitialize)(ExpandEvent)).EndInit();
		}

		private void ExpandEvent_CheckAvailability(Object sender, CheckContentAvailabilityEventArgs ea)
		{
			_EventToExpand = GetEvent(ea.Caret);
			// Exit if no Event found under caret
			if (_EventToExpand == null)
				return;
			// Exit if _Event has Add or Remove
			if (_EventToExpand.Nodes.Count > 0)
				return;
			ea.Available = true;
		}
		private void ExpandEvent_PreparePreview(object sender, PrepareContentPreviewEventArgs ea)
		{
			//ea.AddStrikethrough(_EventToExpand.NameRange);
			//ea.AddCodePreview(_EventToExpand.Range.Start, BuildNewExpandedEventCode("_" + _EventToExpand.Name));
		}
		private void ExpandEvent_Execute(Object sender, ApplyContentEventArgs ea)
		{
			TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
			ActiveDoc.ParseIfTextChanged();
			ActiveDoc.QueueInsert(_EventToExpand.Range.Start, BuildNewEventField(_EventToExpand));
			ActiveDoc.QueueReplace(_EventToExpand, BuildNewExpandedEventCode("_" + _EventToExpand.Name));
			ActiveDoc.ApplyQueuedEdits("Expanded Event", true);
			ActiveDoc.ParseIfTextChanged();
		}
		public void registerCompressEvent()
		{
			DevExpress.Refactor.Core.RefactoringProvider CompressEvent = new DevExpress.Refactor.Core.RefactoringProvider(components);
			((System.ComponentModel.ISupportInitialize)(CompressEvent)).BeginInit();
			CompressEvent.ProviderName = "Compress Event"; // Should be Unique
			CompressEvent.DisplayName = "Compress Event";
			CompressEvent.CheckAvailability += CompressEvent_CheckAvailability;
			CompressEvent.PreparePreview += CompressEvent_PreparePreview;
			CompressEvent.Apply += CompressEvent_Execute;
			((System.ComponentModel.ISupportInitialize)(CompressEvent)).EndInit();
		}
		private void CompressEvent_CheckAvailability(Object sender, CheckContentAvailabilityEventArgs ea)
		{
			TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
			ActiveDoc.ParseIfTextChanged();
			_EventToCompress = GetEvent(ea.Caret);
			// Exit if no Event found under caret
			if (_EventToCompress == null)
				return;
			// Exit if _Event has no Add or Remove
			if (_EventToCompress.Nodes.Count == 0)
				return;

			var EventAdd = (EventAdd)_EventToCompress.Nodes[0];
			var EventRemove = (EventRemove)_EventToCompress.Nodes[1];

			// Add and Remove must contain exactly one assignment statement 
			if (EventAdd.Nodes.Count != 1)
				return;
			if (EventRemove.Nodes.Count != 1)
				return;

			var EventAddAssign = (Assignment)EventAdd.Nodes[0];
			var EventRemoveAssign = (Assignment)EventRemove.Nodes[0];

			// Assignment Statements must assign to same variable
			if (EventAddAssign.LeftSide.Name != EventRemoveAssign.LeftSide.Name)
				return;

			var EventField = EventAddAssign.LeftSide;
			_EventToCompressFieldDeclaration = (Variable)EventField.GetDeclaration().ToLanguageElement();
			// Variable must be of type EventHandler
			if (_EventToCompressFieldDeclaration.MemberType != "EventHandler")
				return;

			// Variable must have 2 and only 2 references.
			if (_EventToCompressFieldDeclaration.FindAllReferences().Count != 2)
				return;

			ea.Available = true;
		}
		private void CompressEvent_PreparePreview(object sender, PrepareContentPreviewEventArgs ea)
		{
			//ea.AddStrikethrough(_EventToCompress.Range);
			//ea.AddCodePreview(_EventToCompress.Range.Start, BuildNewCompressedEventCode(_EventToCompress));
		}
		private void CompressEvent_Execute(Object sender, ApplyContentEventArgs ea)
		{
			TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
			ActiveDoc.ParseIfTextChanged();

			ActiveDoc.QueueDelete(_EventToCompress);
			ActiveDoc.QueueReplace(_EventToCompressFieldDeclaration, BuildNewCompressedEventCode(_EventToCompress));
			ActiveDoc.ApplyQueuedEdits("Compressed Event");
			ActiveDoc.ParseIfTextChanged();

		}

		private string BuildNewEventField(Event SourceEvent)
		{
			ElementBuilder Builder = new ElementBuilder();
			Variable EventField = Builder.BuildVariable(SourceEvent.MemberType , "_" + SourceEvent.Name);
			EventField.Visibility = _EventToExpand.Visibility;
			return GenerateCode(EventField);
		}
		private string BuildNewExpandedEventCode(string FieldName)
		{
			ElementBuilder Builder = new ElementBuilder();
			Event NewEvent = Builder.BuildEvent(_EventToExpand.Name, _EventToExpand.MemberTypeReference);
			NewEvent.Visibility = _EventToExpand.Visibility;

			EventAdd NewEventAdd = Builder.AddEventAdd(NewEvent);
			Builder.AddAssignment(NewEventAdd, FieldName, "value", AssignmentOperatorType.PlusEquals);
			EventRemove NewEventRemove = Builder.AddEventRemove(NewEvent);
			Builder.AddAssignment(NewEventRemove, FieldName, "value", AssignmentOperatorType.MinusEquals);

			return GenerateCode(NewEvent);
		}

		private string BuildNewCompressedEventCode(Event SourceEvent)
		{
			ElementBuilder Builder = new ElementBuilder();
			Event NewEvent = Builder.BuildEvent(SourceEvent.Name, SourceEvent.MemberType);
			NewEvent.Visibility = SourceEvent.Visibility;
			return GenerateCode(NewEvent);
		}

		private Event GetEvent(SourcePoint Caret)
		{
			LanguageElement Element = CodeRush.Documents.ActiveTextDocument.GetNodeAt(Caret);
			if (Element == null)
				return null;
			return Element.GetParentEvent();
		}
		private static string GenerateCode(LanguageElement Element)
		{
			return CodeRush.CodeMod.GenerateCode(Element, true);
		}
	}
}