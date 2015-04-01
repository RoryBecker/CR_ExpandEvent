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
		private void ExpandEvent_Execute(Object sender, ApplyContentEventArgs ea)
		{
			ElementBuilder Builder = ea.NewElementBuilder();
			Variable NewEventField = Builder.BuildVariable("EventHandler", "_" + _EventToExpand.Name);
			NewEventField.Visibility = _EventToExpand.Visibility;

			// Build Event and Add/Remove
			Event NewEvent = Builder.BuildEvent(_EventToExpand.Name, _EventToExpand.MemberTypeReference);
			NewEvent.Visibility = _EventToExpand.Visibility;
			EventAdd NewEventAdd = Builder.AddEventAdd(NewEvent);
			Builder.AddAssignment(NewEventAdd, NewEventField.Name, "value", AssignmentOperatorType.PlusEquals);
			EventRemove NewEventRemove = Builder.AddEventRemove(NewEvent);
			Builder.AddAssignment(NewEventRemove, NewEventField.Name, "value", AssignmentOperatorType.MinusEquals);

			TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
			ActiveDoc.QueueReplace(_EventToExpand, CodeRush.CodeMod.GenerateCode(NewEvent));
			ActiveDoc.QueueInsert(_EventToExpand.Range.Start, CodeRush.CodeMod.GenerateCode(NewEventField));
			ActiveDoc.ApplyQueuedEdits("Expanded Event", true);
			
		}
		private Event GetEvent(SourcePoint Caret)
		{
			LanguageElement Element = CodeRush.Documents.ActiveTextDocument.GetNodeAt(Caret);
			if (Element == null)
				return null;
			return Element.GetParentEvent();
		}

		public void registerCompressEvent()
		{
			DevExpress.Refactor.Core.RefactoringProvider CompressEvent = new DevExpress.Refactor.Core.RefactoringProvider(components);
			((System.ComponentModel.ISupportInitialize)(CompressEvent)).BeginInit();
			CompressEvent.ProviderName = "Compress Event"; // Should be Unique
			CompressEvent.DisplayName = "Compress Event";
			CompressEvent.CheckAvailability += CompressEvent_CheckAvailability;
			CompressEvent.Apply += CompressEvent_Execute;
			((System.ComponentModel.ISupportInitialize)(CompressEvent)).EndInit();
		}
		private void CompressEvent_CheckAvailability(Object sender, CheckContentAvailabilityEventArgs ea)
		{
			CodeRush.Documents.ActiveTextDocument.ParseIfNeeded();
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
		private void CompressEvent_Execute(Object sender, ApplyContentEventArgs ea)
		{
			ElementBuilder Builder = ea.NewElementBuilder();

			// Build Event and Add/Remove
			Event NewEvent = Builder.BuildEvent(_EventToCompress.Name, _EventToCompress.MemberTypeReference);
			NewEvent.Visibility = _EventToCompress.Visibility;

			TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
			ActiveDoc.QueueDelete(_EventToCompressFieldDeclaration);
			ActiveDoc.QueueReplace(_EventToCompress, CodeRush.CodeMod.GenerateCode(NewEvent, true));
			ActiveDoc.ApplyQueuedEdits("Compressed Event");
		}
		


		// Example code
		public event EventHandler MyEvent;

		private EventHandler _myEvent2;
		public event EventHandler MyEvent2
		{
			add
			{
				_myEvent2 += value;
			}
			remove
			{
				_myEvent2 -= value;
			}
		}
	}
}