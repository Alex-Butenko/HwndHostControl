using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
namespace ExampleApp {
	class Program : Form {
		[STAThread]
		static void Main () {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Program());
		}
		TextBox m_textBox;
		Program () {
			int x = 0;
			int y = 0;
			foreach (string line in new string[] {
                "1234567890", "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM "}) {
				foreach (char cur in line) {
					Button button = new Button();
					button.Location = new Point(x * 25, y * 25);
					button.Size = new Size(23, 23);
					button.Text = cur.ToString();
					button.Click += new EventHandler(Button_Click);
					Controls.Add(button);
					x++;
				}
				x = 0;
				y++;
			}
			m_textBox = new TextBox();
			m_textBox.Top = 25 * 4;
			m_textBox.Size = new Size(25 * 10, 23);
			Controls.Add(m_textBox);
			ClientSize = new Size(25 * 10, 25 * 5);
			TopMost = true;
		}
		void Button_Click (object sender, EventArgs e) {
			m_textBox.Text = ((Button) sender).Text;
			SendKeys.Send(m_textBox.Text);
		}
		const int WS_EX_NOACTIVATE = 0x8000000;
		protected override CreateParams CreateParams {
			get {
				CreateParams ret = base.CreateParams;
				ret.ExStyle |= WS_EX_NOACTIVATE;
				return ret;
			}
		}
	}
}