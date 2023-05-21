using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImGuiTools
{
	public interface IFieldLister
	{
		/// <summary>
		///   Enumerates all fields and their values for the given object.
		/// </summary>
		/// <param name="type">type of the object</param>
		/// <param name="value">reference to the object instance</param>
		/// <returns>collection of (FieldInfo, field value)</returns>
		IEnumerable<(FieldInfo, object)> GetFields( System.Type type, object value );

		/// <summary>
		///   For indexable collection types, returns the value at the given index.
		/// </summary>
		/// <param name="value">indexable object instance reference</param>
		/// <param name="index">zero based index</param>
		/// <returns></returns>
		object GetElementAtIndex( object value, int index );
		
		/// <summary>
		///   For collection types, returns the number of elements in the collection.
		/// </summary>
		/// <param name="value">collection object reference</param>
		/// <returns>number of elements</returns>
		int GetElementCount( object value );
	}

		
	/// <summary>
	///   Reflection-based field lister
	/// </summary>
	public class ReflectionFieldLister : IFieldLister
	{
		bool _includeNonPublic;

		public ReflectionFieldLister( bool includeNonPublic=false )
		{
			_includeNonPublic = includeNonPublic;
		}

		public IEnumerable<(FieldInfo, object)> GetFields( System.Type type, object value )
		{
			if( value == null )
				yield break;

			var fieldFlags = BindingFlags.Instance | BindingFlags.Public;
			if( _includeNonPublic )
				fieldFlags |= BindingFlags.NonPublic;

			//var	type = value.GetType();
			var fields = type.GetFields( fieldFlags );

			foreach (var fieldInfo in fields)
			{
				var fieldValue = fieldInfo.GetValue( value );
				yield return (fieldInfo, fieldValue);
			}
		}

		public object GetElementAtIndex( object value, int index )
		{
		    // currently we support just Array
		    // TODO: extend to generic collections/lists
			if( value is not Array arr )
				throw new ArgumentException( "value is not an array" );
			return arr.GetValue( index );
		}

		public int GetElementCount( object value )
		{
		    // currently we support just Array
		    // TODO: extend to generic collections/lists
			if( value is not Array arr )
				throw new ArgumentException( "value is not an array" );
			return arr.Length;
		}
	}
}
