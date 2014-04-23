
// This file has been generated by the GUI designer. Do not modify.
namespace FennecWidget
{
	public partial class DayTimes
	{
		private global::Gtk.Table table1;
		private global::Gtk.Calendar EodDate;
		private global::Gtk.HBox hbox1;
		private global::Gtk.HBox hbox2;
		private global::Gtk.SpinButton spinHour;
		private global::Gtk.SpinButton spinMinute;
		private global::Gtk.Label label1;
		private global::Gtk.Label label2;
		private global::Gtk.Label label3;
		private global::Gtk.Entry txtStartPost;
		private global::Gtk.Button buttonCancel;
		private global::Gtk.Button buttonOk;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget FennecWidget.DayTimes
			this.Name = "FennecWidget.DayTimes";
			this.Title = global::Mono.Unix.Catalog.GetString ("Day Boundaries");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child FennecWidget.DayTimes.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(3)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.EodDate = new global::Gtk.Calendar ();
			this.EodDate.CanFocus = true;
			this.EodDate.Name = "EodDate";
			this.EodDate.DisplayOptions = ((global::Gtk.CalendarDisplayOptions)(35));
			this.table1.Add (this.EodDate);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1 [this.EodDate]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.spinHour = new global::Gtk.SpinButton (0, 100, 1);
			this.spinHour.CanFocus = true;
			this.spinHour.Name = "spinHour";
			this.spinHour.Adjustment.PageIncrement = 10;
			this.spinHour.ClimbRate = 1;
			this.spinHour.Numeric = true;
			this.hbox2.Add (this.spinHour);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.spinHour]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.spinMinute = new global::Gtk.SpinButton (0, 100, 1);
			this.spinMinute.CanFocus = true;
			this.spinMinute.Name = "spinMinute";
			this.spinMinute.Adjustment.PageIncrement = 10;
			this.spinMinute.ClimbRate = 1;
			this.spinMinute.Numeric = true;
			this.hbox2.Add (this.spinMinute);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.spinMinute]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.hbox1.Add (this.hbox2);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.hbox2]));
			w5.Position = 0;
			this.table1.Add (this.hbox1);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1 [this.hbox1]));
			w6.TopAttach = ((uint)(2));
			w6.BottomAttach = ((uint)(3));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("End Date");
			this.table1.Add (this.label1);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1 [this.label1]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Start Post");
			this.table1.Add (this.label2);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1 [this.label2]));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label ();
			this.label3.Name = "label3";
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString ("End Time");
			this.table1.Add (this.label3);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1 [this.label3]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.txtStartPost = new global::Gtk.Entry ();
			this.txtStartPost.CanFocus = true;
			this.txtStartPost.Name = "txtStartPost";
			this.txtStartPost.IsEditable = true;
			this.txtStartPost.InvisibleChar = '●';
			this.table1.Add (this.txtStartPost);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1 [this.txtStartPost]));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			w1.Add (this.table1);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(w1 [this.table1]));
			w11.Position = 0;
			w11.Expand = false;
			w11.Fill = false;
			// Internal child FennecWidget.DayTimes.ActionArea
			global::Gtk.HButtonBox w12 = this.ActionArea;
			w12.Name = "dialog1_ActionArea";
			w12.Spacing = 10;
			w12.BorderWidth = ((uint)(8));
			w12.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w13 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w12 [this.buttonCancel]));
			w13.Expand = false;
			w13.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w14 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w12 [this.buttonOk]));
			w14.Position = 1;
			w14.Expand = false;
			w14.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 308;
			this.DefaultHeight = 330;
			this.Show ();
		}
	}
}
