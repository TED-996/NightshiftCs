using System;
using System.IO;
using System.Text;

namespace NightshiftPlugin {
    public class ActionTextWriter : TextWriter {

        public override Encoding Encoding => Encoding.Default;

        readonly Action<string> routine;
        readonly StringBuilder builder;

        public ActionTextWriter(Action<string> newRoutine) {
            routine = newRoutine;
            builder = new StringBuilder();
        }

        public override void Write(char value) {
            builder.Append(value);
            if (Environment.NewLine.IndexOf(value) != -1) {
                if (builder.Length != 1) {
                    routine(builder.ToString());
                }
                builder.Clear();
            }
        }

        protected override void Dispose(bool disposing) {
            Write('\n');
            base.Dispose(disposing);
        }
    }
}