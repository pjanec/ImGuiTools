
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace ImGuiTools
{
	// Supports arrays (not generic lists!)
	public class ConsoleDump
	{
		public delegate void DumpDeleg( string path, System.Type type, object value, int indent );
		public delegate IEnumerable<(FieldInfo, object)> FieldLister( System.Type type, object value );

		Dictionary<System.Type, DumpDeleg> _dumpersByType;
		Dictionary<string, DumpDeleg> _dumperByPath;
		FieldLister _fieldLister;

		public ConsoleDump(
			Dictionary<System.Type, DumpDeleg> dumpersByType = null,
			Dictionary<string, DumpDeleg> dumperByPath = null,
			FieldLister fieldLister = null
		)
		{
			_dumpersByType = dumpersByType ?? new Dictionary<Type, DumpDeleg>();
			_dumperByPath = dumperByPath ?? new Dictionary<string, DumpDeleg>();
			_fieldLister = fieldLister ?? ReflectionFieldLister;
		}

		public static IEnumerable<(FieldInfo, object)> ReflectionFieldLister( System.Type type, object value )
		{
			var fields = type.GetFields(
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.NonPublic
				);
			foreach (var fieldInfo in fields)
			{
				var fieldValue = fieldInfo.GetValue( value );
				yield return (fieldInfo, fieldValue);
			}
		}

		public void Indent( int level )
		{
			for(int i = 0; i < level; i++ )
				Console.Write( "  " );
		}

		public void Dump( string path, System.Type type, object value, int indent, bool sameLine )
		{
			if( !sameLine )
				Indent( indent );

			if( _dumperByPath.TryGetValue( path, out var dumper ) )
			{
				dumper( path, type, value, indent );
				return;
			}
			
			if( _dumpersByType.TryGetValue( type, out dumper ) )
			{
				dumper( path, type, value, indent );
				return;
			}

			if( type.IsPrimitive )
			{
				DumpPrimitive( path, type, value );
				return;
			}
			if ( type == typeof(System.String) )
			{
				DumpString( path, type, value );
				return;
			}

			if ( type.IsEnum )
			{
				DumpEnum( path, type, value );
				return;
			}

			if ( type == typeof(System.DateTime) )
			{
				DumpPrimitive( path, type, value );
				return;
			}

			if ( type == typeof(System.TimeSpan) )
			{
				DumpPrimitive( path, type, value );
				return;
			}

			if ( type == typeof(System.Guid) )
			{
				DumpPrimitive( path, type, value );
				return;
			}

			if ( type.IsArray )
			{
				DumpArray( path, type, value, indent );
				return;
			}

			//if( ReflectionUtility.IsList( type ) )
			//{
			//	DumpList( type, value, indent );
			//	return;
			//}

			if( type.IsClass )
			{
				DumpClass( path, type, value, indent );
				return;
			}

			if( type.IsValueType )
			{
				DumpClass( path, type, value, indent );
			}
		}

		public void DumpClass( string path, Type type, object value, int indent )
		{
			if (value != null)
			{
				Console.WriteLine( $"[{(type.IsValueType ? "Struct" : "Class")} {type.Name}]" );
				foreach(var (fieldInfo, fieldValue) in _fieldLister(type, value) )
				{
					var fieldType = fieldInfo.FieldType;

					var childPath = string.IsNullOrEmpty(path)
						? fieldInfo.Name
						: path + "." + fieldInfo.Name;

					Indent( indent + 1 );
					Console.Write( $"{fieldInfo.Name} = " );
					Dump( childPath, fieldType, fieldValue, indent + 1, true );
				}
			}
			else
			{
				Console.WriteLine( $"[Class {type.Name}] = null" );
			}
		}

		public void DumpArray( string path, Type type, object value, int indent )
		{
			var elemType = type.GetElementType();
			var arr = value as Array;
			if (arr != null)
			{
				int length = arr.Length;
				Console.WriteLine( $"[Array of {elemType.Name}] ({length})" );
				for (int i = 0; i < length; i++)
				{
					Indent( indent + 1 );
					Console.Write( $"[{i}] = " );
					var elemValue = arr.GetValue( i );
					Dump( path, elemType, elemValue, indent + 1, true );
				}
			}
			else
			{
				Console.WriteLine( $"[Array of {elemType.Name}] = null" );
			}
		}

		private void DumpEnum( string path, Type type, object value )
		{
			Console.WriteLine( $"[enum {type.Name}] {value}" );
		}

		private void DumpString( string path, Type type, object value )
		{
			if (value == null)
				Console.WriteLine( $"[{type.Name}] null" );
			else
				Console.WriteLine( $"[{type.Name}] \"{value}\"" );
		}

		private void DumpPrimitive( string path, Type type, object value )
		{
			Console.WriteLine( $"[{type.Name}] {value}" );
			return;
		}
	}
}
