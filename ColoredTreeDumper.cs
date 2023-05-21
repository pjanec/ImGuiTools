using System;
using System.Collections.Generic;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Numerics;
using System.Reflection;
using SixLabors.ImageSharp;

namespace ImGuiTools
{
	// 
	/// <summary>
	/// Renders the content of object of givent type & value as a tree with colored parts (field names, numbers, strings...)
	/// Allows for custom renderer for given type or for fields on specific path within the object (like "field1.subfield2.subsubfield3").
	/// </summary>
	public class ColoredTreeDumper
	{
		/// <summary>
		///   Delegate rendering the field of given type & value.
		/// </summary>
		/// <remarks>
		///   The delegate is expected to render the field value using ImGui.
		///   If it is a container (array, list, dictionary, etc.), it should render expandable TreeNode as well as the the contained items.
		///   If it is not a container, it should render the value as a single line, but indented to left align with the TreeNode fields.
		///   The 'key' delegate is provided to render the part on the left (usualy the name of the field); you can apply imgui style
		///   if you want it to have certain color.
		/// </remarks>
		/// <param name="path">where within the object's internal hierarchy the field is. (like "field1.subfield2.subsubfield3")</param>
		/// <param name="type">object value type</param>
		/// <param name="value">reference to the object</param>
		/// <param name="key">what to render of the left; usually the field name; null = nothing</param>
		public delegate void DumpDeleg( string path, System.Type type, Action key, object value );

		public delegate void RenderedDeleg( string path, System.Type type, object value );

		/// <summary>
		///   Custom renderer for given field type
		/// </summary>
		public Dictionary<System.Type, DumpDeleg> DumpersByType = new Dictionary<Type, DumpDeleg>();
		
		/// <summary>
		///   Custom renderer for fields on specific path within the object (like "field1.subfield2.subsubfield3").
		/// </summary>
		public Dictionary<string, DumpDeleg> DumpersByPath = new Dictionary<string, DumpDeleg>();
		
		/// <summary>
		///   How to list the fields of given object type.
		/// </summary>
		public IFieldLister Lister = new ReflectionFieldLister();

		/// <summary>
		///   Called after the key has been rendered. You can check for hover, clicks etc. here.
		/// </summary>
		public RenderedDeleg OnKeyRendered = null; 

		/// <summary>
		///   Called after the value has been rendered. You can check for hover, clicks etc. here.
		/// </summary>
		public RenderedDeleg OnValueRendered = null; 

		/// <summary>
		///   Should all containers be rendered as expanded by default?
		/// </summary>
		public bool DefaultExpanded = false;


		static Vector4 ByteToFloat( Vector4 b ) => new Vector4( (float)b.X / 255.0f, (float)b.Y / 255.0f, (float)b.Z / 255.0f, (float)b.W / 255.0f );
		
		public class ColorSpec
		{
			public Vector4 Number = ByteToFloat( new Vector4(210,238,38,255));
			public Vector4 Index = ByteToFloat( new Vector4(175,175,175,255));
			public Vector4 TypeName = ByteToFloat( new Vector4(175,175,175,255));
			public Vector4 ContainerField = ByteToFloat( new Vector4(237,125,49,255));
			public Vector4 ArrayLength = ByteToFloat( new Vector4(240,240,240,240));
			public Vector4 SimpleField = ByteToFloat( new Vector4(156,220,254,255));
			public Vector4 String = ByteToFloat( new Vector4(248,0,251,255));
			public Vector4 EnumItem = ByteToFloat( new Vector4(144,247,35,255));
			public Vector4 EqualSign = ByteToFloat( new Vector4(175,175,175,255));
			public Vector4 Brackets = ByteToFloat( new Vector4(230,230,230,255));
			public Vector4 Commas = ByteToFloat( new Vector4(230,230,230,255));
			public Vector4 Triangle = ByteToFloat( new Vector4(143,170,220,255));
			public Vector4 Constant = ByteToFloat( new Vector4(0,156,255,255));
		}

		public ColorSpec Colors = new ColorSpec();

		public ColoredTreeDumper()
		{
		}

		/// <summary>
		///    Draws the object as a colored tree.
		/// </summary>
		/// <param name="path">field hierarchy path (like "field1.subfield2.subsubfield3"); comma separated levels; should be empty if at root level</param>
		/// <param name="type">type of the object to draw</param>
		/// <param name="value">reference to the object to draw</param>
		/// <param name="key">action to render the part on the left (usualy the name of the field); null = nothing is rendered prior to the value</param>
		public void Draw( string path, System.Type type, Action key, object value )
		{
			ImGui.PushID(path);	// isolate popups etc

			if( DumpersByPath.TryGetValue( path, out var dumper ) )
			{
				dumper( path, type, key, value );
			}
			else
			if( DumpersByType.TryGetValue( type, out dumper ) )
			{
				dumper( path, type, key, value );
			}
			else
			if ( type == typeof(System.String) )
			{
				DumpString( path, type, key, value );
			}
			else
			if ( type == typeof(System.Boolean) )
			{
				DumpBoolean( path, type, key, value );
			}
			else
			if ( type.IsEnum )
			{
				DumpEnum( path, type, key, value );
			}
			else
			if ( type == typeof(System.DateTime) )
			{
				DumpPrimitive( path, type, key, value );
			}
			else
			if ( type == typeof(System.TimeSpan) )
			{
				DumpPrimitive( path, type, key, value );
			}
			else
			if ( type == typeof(System.Guid) )
			{
				DumpPrimitive( path, type, key, value );
			}
			else
			if( type.IsPrimitive )
			{
				DumpPrimitive( path, type, key, value );
			}
			else
			if ( type.IsArray )
			{
				DumpArray( path, type, key, value );
			}
			else
			if( type.IsClass )
			{
				DumpClass( path, type, key, value );
			}
			else
			if( type.IsValueType )
			{
				DumpClass( path, type, key, value );
			}

			ImGui.PopID();
		}

		public void DrawKey( string path, System.Type type, Action key, object value, Vector4 color )
		{
			ImGui.PushStyleColor(ImGuiCol.Text, color);
			key();
			ImGui.PopStyleColor();

			OnKeyRendered?.Invoke( path, type, value );
		}

		public void DrawValue( string path, System.Type type, object value, bool withEqualSign, bool quoted )
		{
			if( withEqualSign )
			{
				ImGui.TextColored(Colors.EqualSign, "=");
				ImGui.SameLine();
			}
			
			ImGui.BeginGroup();

			if( value is Action action )
			{
				action();
			}
			else
			{
				DrawValueAsText( value, quoted );
			}

			ImGui.EndGroup();
			
			OnValueRendered?.Invoke( path, type, value );
		}

		public void DrawValue( string path, System.Type type, object value, bool withEqualSign, bool quoted, Vector4 color )
		{
			if( color == Vector4.Zero )	// no color -> use default
			{
				DrawValue( path, type, value, withEqualSign, quoted );
			}
			else
			{
				ImGui.PushStyleColor(ImGuiCol.Text, color);
				DrawValue( path, type, value, withEqualSign, quoted );
				ImGui.PopStyleColor();
			}
		}

		public void DrawValueAsText( object value, bool quoted )
		{
			if( value == null )
			{
				ImGui.TextColored(Colors.Constant, "null" );
			}
			else
			{
				if( quoted )
				{
					ImGui.Text( $"\"{value}\"" );
				}
				else
				{
					ImGui.Text(value.ToString() );
				}
			}
		}

		public void DrawKeyValue( string path, System.Type type, Action key, object value, bool withEqualSign, bool quoted, Vector4 keyColor, Vector4 valueColor )
		{
			ImGui.Indent();
			
			if( key != null )
			{
				DrawKey( path, type, key, value, keyColor );
			
				ImGui.SameLine();
			}
			
			DrawValue( path, type, value, key != null, quoted, valueColor );
			
			ImGui.Unindent();
		}


		public void DumpClass( string path, Type type, Action key, object value )
		{
			if (value != null)
			{
				if( key != null )
				{
					ImGui.PushStyleColor(ImGuiCol.Text, Colors.Triangle);
					bool opened = ImGui.TreeNodeEx( $"##{path}", DefaultExpanded ? ImGuiTreeNodeFlags.DefaultOpen : 0 );
					ImGui.PopStyleColor();
				
					if( key != null )
					{
						ImGui.SameLine();
					
						DrawKey(path, type, key, value, Colors.ContainerField);
					}

					if( opened )
					{
						DrawClassFields( path, type, value );

						ImGui.TreePop();
					}
				}
				else
				{
					DrawClassFields( path, type, value );					
				}
			}
			else
			{
				DrawKeyValue( path, type, key, value, true, false, Colors.ContainerField, Vector4.Zero );
			}
		}

		public void DrawClassFields( string path, Type type, object value )
		{
			foreach (var (fieldInfo, fieldValue) in Lister.GetFields( type, value ))
			{
				var fieldType = fieldInfo.FieldType;

				var childPath = string.IsNullOrEmpty( path )
					? fieldInfo.Name
					: path + "." + fieldInfo.Name;

				Action childKey = () =>
				{
					ImGui.Text( $"{fieldInfo.Name}" );
				};
				Draw( childPath, fieldType, childKey, fieldValue );
			}
		}

		// showing custom formatted value inline, but expandable to show all fields
		public void DrawClassKeyValue( string path, Type type, Action key, object value, object inlineValue )
		{
			ImGui.PushStyleColor(ImGuiCol.Text, Colors.Triangle);
			bool opened = ImGui.TreeNodeEx( $"##{path}" ); // by default closed because we are drawing custom format on same line
			ImGui.PopStyleColor();
				
			if( key != null )
			{
				ImGui.SameLine();
					
				DrawKey(path, type, key, value, Colors.ContainerField);
			}

			ImGui.SameLine();

			DrawValue( path, type, inlineValue, true, false, Vector4.Zero );

			if( opened )
			{
				DrawClassFields( path, type, value );

				ImGui.TreePop();
			}
		}

		public void DumpArray( string path, Type type, Action key, object value )
		{
			var elemType = type.GetElementType();
			if (value != null)
			{
				int length = Lister.GetElementCount(value);

				if( key != null )
				{
					ImGui.PushStyleColor(ImGuiCol.Text, Colors.Triangle);
					bool opened = ImGui.TreeNodeEx( $"##{path}", DefaultExpanded ? ImGuiTreeNodeFlags.DefaultOpen : 0 );
					ImGui.PopStyleColor();
				
					if( key != null )
					{
						ImGui.SameLine();
						DrawKey( path, type, key, value, Colors.ContainerField );
					}

					ImGui.SameLine();
					ImGui.TextColored(Colors.ArrayLength, $"({length})");
				
					if( opened)
					{
						DrawArrayElements( path, value, elemType, length );

						ImGui.TreePop();
					}
				}
				else
				{
					DrawArrayElements( path, value, elemType, length );
				}
			}
			else
			{
				DrawKeyValue( path, type, key, value, false, false, Colors.ContainerField, Vector4.Zero );
			}
		}

		private void DrawArrayElements( string path, object value, Type elemType, int length )
		{
			for (int i = 0; i < length; i++)
			{
				Action childKey = () =>
				{
					ImGui.TextColored( Colors.Index, $"[{i}]" );
				};
				var elemValue = Lister.GetElementAtIndex( value, i );
				Draw( path+$"[{i}]", elemType, childKey, elemValue );
			}
		}

		private void DumpEnum( string path, Type type, Action key, object value )
		{
			DrawKeyValue( path, type, key, value, true, false, Colors.SimpleField, Colors.EnumItem );
		}

		private void DumpString( string path, Type type, Action key, object value )
		{
			DrawKeyValue( path, type, key, value, true, true, Colors.SimpleField, Colors.String );
		}

		private void DumpBoolean( string path, Type type, Action key, object value )
		{
			DrawKeyValue( path, type, key, value, true, false, Colors.SimpleField, Colors.Constant );
		}

		private void DumpPrimitive( string path, Type type, Action key, object value )
		{
			DrawKeyValue( path, type, key, value, true, false, Colors.SimpleField, Colors.Number );
		}

		public void DrawStyleSetting()
		{
			DrawColorPicker("Number", ref Colors.Number); ImGui.SameLine();
			DrawColorPicker("Index", ref Colors.Index); ImGui.SameLine();
			DrawColorPicker("TypeName", ref Colors.TypeName); ImGui.SameLine();
			DrawColorPicker("ContainerField", ref Colors.ContainerField); ImGui.SameLine();
			DrawColorPicker("ArrayLength", ref Colors.ArrayLength); ImGui.SameLine();
			DrawColorPicker("SimpleField", ref Colors.SimpleField); ImGui.SameLine();
			DrawColorPicker("String", ref Colors.String); ImGui.SameLine();
			DrawColorPicker("EnumItem", ref Colors.EnumItem); ImGui.SameLine();
			DrawColorPicker("EqualSign", ref Colors.EqualSign); ImGui.SameLine();
			DrawColorPicker("Brackets", ref Colors.Brackets); ImGui.SameLine();
			DrawColorPicker("Commas", ref Colors.Commas); ImGui.SameLine();
			DrawColorPicker("Triangle", ref Colors.Triangle); ImGui.SameLine();
			DrawColorPicker("Constant", ref Colors.Constant);
		}
		
		public void DrawColorPicker( string name, ref Vector4 color )
		{
			bool open = ImGui.ColorButton( name, color );
			if( open )
			{
				ImGui.OpenPopup( "mypicker"+name );
			}
			if( ImGui.BeginPopup( "mypicker"+name ) )
			{
				if( ImGui.ColorPicker4( name, ref color ) )
				{
				}
				ImGui.EndPopup();
			}
		}

	}
}
