using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Drawing;
using ImGuiNET;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Fasterflect;
using System.Diagnostics;

namespace ImGuiTools
{
	public class GuiWindow : Disposable
	{
		public bool HasMenu => true;

		private ImGuiWindow _wnd;


		// sample data structure we are using Fastreflect for retrieving the members
		class MyData
		{
			public string Field1;
			public int Field2;
		}


		public GuiWindow( ImGuiWindow wnd )
		{
			_wnd = wnd;

		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		public void Tick()
		{
		}

		public void DrawUI()
		{

			float ww = ImGui.GetWindowWidth();

			//_menuRenderer.DrawUI();

			if( ImGui.BeginTabBar("MainTabBar") )
			{
				if( ImGui.BeginTabItem("Tab1") )
				{
					//DrawApps();
					ImGui.EndTabItem();
				}
				ImGui.EndTabBar();
			}

			Test1();







		}


		enum MyEnum
		{
			Enum1,
			Enum2
		}

		class MyStruct
		{
			public int IntField;
		}

		class MyClass
		{
			public int IntField;
			public string StringField;
			public MyEnum MyEnum;
			public Guid GuidField;
			public MyStruct StructField;
			public MyStruct[] StructArray;
			public List<MyStruct> StructList;
			public DateTime DateTimeField;
			public MyClass MyClass2;
			public Action<int> SomeIntAction;
		}

		ImGuiTreeDump dumper = new ImGuiTreeDump(); // captured
		void Test1()
		{
			var myClass = new MyClass();
			myClass.IntField = 1;
			myClass.StringField = "Hello!";
			myClass.MyEnum = MyEnum.Enum2; 
			myClass.GuidField = Guid.NewGuid();
			myClass.StructField = new MyStruct() { IntField = 6 };
			myClass.StructArray = new MyStruct[1] { new MyStruct { IntField = 7 } };
			myClass.StructList = new List<MyStruct>() { new MyStruct { IntField = 8 } };
			myClass.DateTimeField = DateTime.Now;
			myClass.MyClass2 = new MyClass();

			
			dumper.DumpersByType = new Dictionary<System.Type, ImGuiTreeDump.DumpDeleg>()
			{
				{ typeof(DateTime), (path, type, value, key) => {
					ImGui.Indent();
					if( key != null )
					{
						ImGui.PushStyleColor(ImGuiCol.Text, dumper.Colors.SimpleField);
						key();
						ImGui.PopStyleColor();

						ImGui.SameLine();
					}
					ImGui.TextColored(dumper.Colors.EqualSign, "=");
					ImGui.SameLine();
					ImGui.TextColored(dumper.Colors.TypeName, $"[{type.Name}]");
					ImGui.SameLine();
					ImGui.TextColored(dumper.Colors.Number,$"CUSTOM {value:HH:mm:ss.fff}");
					ImGui.Unindent();
				} },
			};
			
			dumper.DumpersByPath = new Dictionary<string, ImGuiTreeDump.DumpDeleg>()
			{
				{ "StructArray.IntField", (path, type, value, key) =>	{
					ImGui.Indent();
					if( key != null )
					{
						ImGui.PushStyleColor(ImGuiCol.Text, dumper.Colors.SimpleField);
						key();
						ImGui.PopStyleColor();

						ImGui.SameLine();
					}
					ImGui.TextColored(dumper.Colors.EqualSign, "=");
					ImGui.SameLine();
					ImGui.TextColored(dumper.Colors.TypeName, $"[{type.Name}]");
					ImGui.SameLine();
					ImGui.TextColored(dumper.Colors.Number,$"CUSTOM {value}");
					ImGui.Unindent();
				}},
			};

			dumper.DrawStyleSetting();

			dumper.Dump( "", typeof(MyClass), myClass, () => ImGui.Text("MyClass") );
			//dumper.Dump( "", typeof(MyClass), myClass, null );



		}


	}
}
