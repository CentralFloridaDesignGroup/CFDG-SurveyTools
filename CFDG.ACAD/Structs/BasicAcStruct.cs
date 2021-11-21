using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace CFDG.ACAD
{
    public struct AcVariablesStruct
    {
        public Document Document;

        public Editor Editor;

        public Database Database;
    }
}
