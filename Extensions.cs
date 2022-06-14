using System.Windows.Forms;

namespace ComboSkills {
    public static class Extensions {
		public static void UI(this Control control, MethodInvoker code) {
			if(control.InvokeRequired) {
				control.Invoke(code);
			} else {
				code.Invoke();
			}
		}
	}
}
