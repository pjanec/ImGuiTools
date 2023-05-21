
using SharpDX.Direct3D11;
using System.Collections.Generic;

namespace ImGuiTools
{
	internal static class Program
	{
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

		static void Test1()
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

			var typeDumpers = new Dictionary<System.Type, ConsoleDump.DumpDeleg>()
			{
				{ typeof(DateTime), (path, type, value, indent) => Console.WriteLine( $"[{type.Name}] {value:HH:mm:ss.fff}" ) },
			};
			
			var pathDumpers = new Dictionary<string, ConsoleDump.DumpDeleg>()
			{
				{ "StructArray.IntField", (path, type, value, indent) => Console.WriteLine( $"[{type.Name}] CUSTOM {value}" ) },
			};

			var dumper = new ConsoleDump(typeDumpers, pathDumpers);
			dumper.Dump( "", typeof(MyClass), myClass, 0, false );

		}

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			//// To customize application configuration such as set high DPI settings or default font,
			//// see https://aka.ms/applicationconfiguration.
			//ApplicationConfiguration.Initialize();
			//Application.Run( new Form1() );

			var app = new GuiApp();
			app.run();
			app.Dispose();

			//Test1();

		}

	}
}