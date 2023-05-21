using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImGuiTools
{
	public static class ReflectionCache
	{
		// Parallel type info hierarchy to the built-in reflection,
		// providing faster access to the members of a type
		public class MyFieldInfo
		{
			public FieldInfo NativeFieldInfo;
			public MyType MyType;
			public Fasterflect.MemberGetter ValueGetter;
			public Fasterflect.ArrayElementGetter ArrayElementGetter;
		}
		
		public class MyType
		{
			public System.Type Type;
			public List<MyFieldInfo> Fields;
		}

		static Dictionary<System.Type, MyType> _typeCache = new Dictionary<System.Type, MyType>();

		static MyType ScanType( System.Type type, Fasterflect.MemberGetter parentGetter )
		{
			if( _typeCache.TryGetValue( type, out var myType ) )
				return myType;

			myType = new MyType();
			myType.Type = type;
			myType.Fields = new List<MyFieldInfo>();
			foreach( var field in type.GetFields() )
			{
				var myField = new MyFieldInfo();
				myField.NativeFieldInfo = field;
				myField.MyType = ScanType( field.FieldType, parentGetter );
				//myField.ValueGetter = 
				//myField.ArrayElementGetter = 
				myType.Fields.Add( myField );
			}

			_typeCache.Add( type, myType );

			return myType;
		}

		public static MyType Get( System.Type type)
		{
			if( _typeCache.TryGetValue( type, out var myType ) ) return myType;
			//myType = ScanType;
			return myType;
		}

	}
}
