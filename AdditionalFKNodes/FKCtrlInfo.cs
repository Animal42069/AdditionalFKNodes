#if AI || HS2
using AIChara;
#endif
using Studio;

namespace AdditionalFKNodes
{
    internal class FKCtrlInfo
    {
        internal FKCtrl fkCtrl;
        internal OCIChar ociChar;
        internal OICharInfo info;
        internal ChaControl chaControl;
        internal ChaReference charReference;

        internal FKCtrlInfo(FKCtrl fkCtrl, OCIChar ociChar, OICharInfo info, ChaControl chaControl, ChaReference charReference)
        {
            this.fkCtrl = fkCtrl;
            this.ociChar = ociChar;
            this.info = info;
            this.chaControl = chaControl;
            this.charReference = charReference;
        }

        internal FKCtrlInfo(FKCtrl fkCtrl, OICharInfo info, ChaControl chaControl, ChaReference charReference)
        {
            this.fkCtrl = fkCtrl;
            this.info = info;
            this.chaControl = chaControl;
            this.charReference = charReference;
        }

        internal FKCtrlInfo(OCIChar ociChar, ChaControl chaControl)
        {
            this.ociChar = ociChar;
            this.chaControl = chaControl;
        }
    }
}
