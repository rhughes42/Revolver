using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Revolver
{
    public class RevolverInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Revolver";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Synchronous evolutionary optimisation solver for Rhino Compute.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("33db7fc7-018b-4848-864d-d6f73212ed70");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Schmidt Hammer Lassen Architects";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "rhu@shl.dk";
            }
        }
    }
}
