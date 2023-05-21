using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Drawing;
using ImGuiNET;
using System.Threading;
using ImVec2 = System.Numerics.Vector2;

namespace ImGuiTools
{
	public class GuiApp : Disposable
	{
		private ImGuiWindow _wnd;
		private string _uniqueUiId = Guid.NewGuid().ToString();
		private GuiWindow _guiWin;

		public GuiApp()
		{
			_wnd = new ImGuiWindow("Gui", width:800, height:650);
			_wnd.OnDrawUI += DrawUI;

			_guiWin = new GuiWindow( _wnd );


		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if( !disposing ) return;

			_guiWin?.Dispose();
			_wnd.Dispose();
		}

		public int run()
		{
			while( _wnd.Exists )
			{
				_wnd.Tick();

				_guiWin?.Tick();

				Thread.Sleep( 50 );
			}

			return 0;
		}

		void DrawUI()
		{
			ImGui.SetNextWindowPos(new ImVec2(0, 0));
			ImGui.SetNextWindowSize(new ImVec2(_wnd.Size.X, _wnd.Size.Y));
			if (_guiWin != null)
			{
				if (ImGui.Begin("Gui", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | (_guiWin.HasMenu?ImGuiWindowFlags.MenuBar:0) ))
				{
					_guiWin?.DrawUI();
					ImGui.End();
				}
			}
		}

		
	}
}
