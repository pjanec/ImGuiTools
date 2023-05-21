
using ImGuiNET;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Reflection;

namespace ImGuiTools
{
	// Supports arrays (not generic lists!)
	public class ImGuiTreeDump
	{
		public delegate void DumpDeleg( string path, System.Type type, object value, Action key );
		public delegate IEnumerable<(FieldInfo, object)> FieldLister( System.Type type, object value );

		public Dictionary<System.Type, DumpDeleg> DumpersByType = new Dictionary<Type, DumpDeleg>();
		public Dictionary<string, DumpDeleg> DumpersByPath = new Dictionary<string, DumpDeleg>();
		public ILister Lister = new ReflectionLister();

		static Vector4 ByteToFloat( Vector4 b ) => new Vector4( (float)b.X / 255.0f, (float)b.Y / 255.0f, (float)b.Z / 255.0f, (float)b.W / 255.0f );
		
		public class ColorSpec
		{
			public Vector4 Default = ByteToFloat( new Vector4(250,250,250,255));
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

		public ImGuiTreeDump()
		{
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <param name="key">action to render the part on the left (usualy the name of the field); null = nothing is rendered prior to the value</param>
		public void Dump( string path, System.Type type, object value, Action key )
		{
			if( DumpersByPath.TryGetValue( path, out var dumper ) )
			{
				dumper( path, type, value, key );
				return;
			}
			
			if( DumpersByType.TryGetValue( type, out dumper ) )
			{
				dumper( path, type, value, key );
				return;
			}

			if( type.IsPrimitive )
			{
				DumpPrimitive( path, type, value, key );
				return;
			}
			if ( type == typeof(System.String) )
			{
				DumpString( path, type, value, key );
				return;
			}

			if ( type.IsEnum )
			{
				DumpEnum( path, type, value, key );
				return;
			}

			if ( type == typeof(System.DateTime) )
			{
				DumpPrimitive( path, type, value, key );
				return;
			}

			if ( type == typeof(System.TimeSpan) )
			{
				DumpPrimitive( path, type, value, key );
				return;
			}

			if ( type == typeof(System.Guid) )
			{
				DumpPrimitive( path, type, value, key );
				return;
			}

			if ( type.IsArray )
			{
				DumpArray( path, type, value, key );
				return;
			}

			//if( ReflectionUtility.IsList( type ) )
			//{
			//	DumpList( type, value, indent );
			//	return;
			//}

			if( type.IsClass )
			{
				DumpClass( path, type, value, key );
				return;
			}

			if( type.IsValueType )
			{
				DumpClass( path, type, value, key );
			}
		}

		public void DumpClass( string path, Type type, object value, Action key )
		{
			if (value != null)
			{
				if( key != null )
				{
					ImGui.PushStyleColor(ImGuiCol.Text, Colors.Triangle);
					bool opened = ImGui.TreeNodeEx( $"##{path}", ImGuiTreeNodeFlags.DefaultOpen );
					ImGui.PopStyleColor();
				
					if( key != null )
					{
						ImGui.SameLine();
					
						ImGui.PushStyleColor(ImGuiCol.Text, Colors.ContainerField);
						key();
						ImGui.PopStyleColor();
					}

					//ImGui.SameLine();
					//ImGui.Text($"[Class {type.Name}]" );

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
				ImGui.Indent();

				if( key != null )
				{
					ImGui.PushStyleColor(ImGuiCol.Text, Colors.ContainerField);
					key();
					ImGui.PopStyleColor();
					ImGui.SameLine();
				}

				//ImGui.Text( $"[Class {type.Name}] = null" );
				ImGui.TextColored(Colors.EqualSign, "=");
				ImGui.SameLine();
				ImGui.TextColored(Colors.Constant, "null" );

				ImGui.Unindent();
			}
		}

		private void DrawClassFields( string path, Type type, object value )
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
				Dump( childPath, fieldType, fieldValue, childKey );
			}
		}

		public void DumpArray( string path, Type type, object value, Action key )
		{
			var elemType = type.GetElementType();
			var arr = value as Array;
			if (arr != null)
			{
				ImGui.PushStyleColor(ImGuiCol.Text, Colors.Triangle);
				bool opened = ImGui.TreeNodeEx( $"##{path}", ImGuiTreeNodeFlags.DefaultOpen );
				ImGui.PopStyleColor();
				
				if( key != null )
				{
					ImGui.SameLine();
					ImGui.PushStyleColor(ImGuiCol.Text, Colors.ContainerField);
					key();
					ImGui.PopStyleColor();
				}

				int length = Lister.GetArrayLength(value);
				
				ImGui.SameLine();
				ImGui.TextColored(Colors.ArrayLength, $"({length})");
				
				if( opened)
				{
					for (int i = 0; i < length; i++)
					{
						Action childKey = () =>
						{
							ImGui.TextColored(Colors.Index, $"[{i}]");
						};
						var elemValue = Lister.GetArrayItem( value, i );
						Dump( path, elemType, elemValue, childKey);
					}

					ImGui.TreePop();
				}
			}
			else
			{
				ImGui.Indent();

				if( key != null )
				{
					ImGui.PushStyleColor(ImGuiCol.Text, Colors.ContainerField);
					key();
					ImGui.PopStyleColor();
					ImGui.SameLine();
				}
				

				ImGui.TextColored(Colors.EqualSign, "=");
				ImGui.SameLine();
				ImGui.TextColored(Colors.Constant, "null" );

				ImGui.Unindent();
			}
		}

		private void DumpEnum( string path, Type type, object value, Action key )
		{
			ImGui.Indent();

			if( key != null )
			{
				ImGui.PushStyleColor(ImGuiCol.Text, Colors.SimpleField);
				key();
				ImGui.PopStyleColor();

				ImGui.SameLine();
			}

			ImGui.TextColored(Colors.EqualSign, "=");
			ImGui.SameLine();
			ImGui.TextColored(Colors.EnumItem, $"{value}" );

			ImGui.Unindent();
		}

		private void DumpString( string path, Type type, object value, Action key )
		{
			ImGui.Indent();

			if( key != null )
			{
				ImGui.PushStyleColor(ImGuiCol.Text, Colors.SimpleField);
				key();
				ImGui.PopStyleColor();

				ImGui.SameLine();
			}

			if (value == null)
			{
				ImGui.TextColored(Colors.EqualSign, "=");
				ImGui.SameLine();
				ImGui.TextColored(Colors.Constant, "null");
			}
			else
			{
				ImGui.TextColored(Colors.EqualSign, "=");
				ImGui.SameLine();
				ImGui.TextColored(Colors.String, $"\"{value}\"" );
			}

			ImGui.Unindent();
		}

		private void DumpPrimitive( string path, Type type, object value, Action key )
		{
			ImGui.Indent();

			if( key != null )
			{
				ImGui.PushStyleColor(ImGuiCol.Text, Colors.SimpleField);
				key();
				ImGui.PopStyleColor();

				ImGui.SameLine();
			}

			ImGui.TextColored(Colors.EqualSign, "=");
			ImGui.SameLine();
			ImGui.TextColored(Colors.Number, $"{value}" );

			ImGui.Unindent();
		}

		public void DrawStyleSetting()
		{
			DrawColorPicker("Default", ref Colors.Number); ImGui.SameLine();
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
