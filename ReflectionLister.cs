using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImGuiTools
{
	public interface ILister
	{
		IEnumerable<(FieldInfo, object)> GetFields( System.Type type, object value );
		object GetArrayItem( object value, int index );
		int GetArrayLength( object value );
	}

		
	public class ReflectionLister : ILister
	{
		public IEnumerable<(FieldInfo, object)> GetFields( System.Type type, object value )
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

		public object GetArrayItem( object value, int index )
		{
			if( value is not Array arr )
				throw new ArgumentException( "value is not an array" );
			return arr.GetValue( index );
		}

		public int GetArrayLength( object value )
		{
			if( value is not Array arr )
				throw new ArgumentException( "value is not an array" );
			return arr.Length;
		}
	}
}
