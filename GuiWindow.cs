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
using System.Numerics;

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

			Init();
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

		ColoredTreeDumper dumper;

		void Init()
		{
			dumper = new ColoredTreeDumper(); // captured

			dumper.DefaultExpanded = true;

			dumper.DumpersByType = new Dictionary<System.Type, ColoredTreeDumper.DumpDeleg>()
			{
				{ typeof(DateTime), (path, type, key, value) => {

					Action valueRenderer = () => {
						ImGui.TextColored(dumper.Colors.TypeName, $"[{type.Name}]");
						ImGui.SameLine();
						ImGui.TextColored(dumper.Colors.Number,$"CUSTOM {value:HH:mm:ss.fff}");
					};
					dumper.DrawKeyValue(path, type, key, valueRenderer, true, false, dumper.Colors.SimpleField, Vector4.Zero);
				} },
			};
			
			dumper.DumpersByPath = new Dictionary<string, ColoredTreeDumper.DumpDeleg>()
			{
				{ "StructArray.IntField", (path, type, key, value) =>	{

					Action valueRenderer = () => {
						ImGui.TextColored(dumper.Colors.TypeName, $"[{type.Name}]");
						ImGui.SameLine();
						ImGui.TextColored(dumper.Colors.Number,$"CUSTOM {value}");
					};
					dumper.DrawKeyValue(path, type, key, valueRenderer, true, false, dumper.Colors.SimpleField, Vector4.Zero);
				}},
			};

			dumper.OnKeyRendered = (path, type, value) =>
			{
				if( ImGui.IsItemHovered() )
				{
					ImGui.BeginTooltip();
					ImGui.TextColored(dumper.Colors.String, $"{path}");
					ImGui.SameLine();
					ImGui.TextColored(dumper.Colors.TypeName, $"[{type.Name}]");
					ImGui.EndTooltip();
				}
			};

			dumper.OnValueRendered = (path, type, value) =>
			{
				if( ImGui.IsItemHovered() )
				{
					ImGui.BeginTooltip();
					dumper.DrawValueAsText(value, false);
					ImGui.EndTooltip();
				}
			};

		}

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

			
			dumper.DrawStyleSetting();

			//dumper.Draw( "", typeof(MyClass), () => ImGui.Text("MyClass"), myClass );
			dumper.Draw( "", typeof(MyClass), null, myClass );
			//dumper.Draw( "", typeof(MyClass), null, null );
			//dumper.Draw( "", typeof(MyClass), () => ImGui.Text("MyClass"), null );



		}


	}
}
