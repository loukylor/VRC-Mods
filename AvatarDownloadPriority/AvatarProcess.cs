namespace AvatarDownloadPriority
{
    public class AvatarProcess
    {
        public VRCAvatarManager manager;
        public object[] methodParams;
        public string Id => manager.field_Private_VRCPlayer_0.prop_Player_0.prop_APIUser_0.id;
        public string DisplayName => manager.field_Private_VRCPlayer_0.prop_Player_0.prop_APIUser_0.displayName;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            AvatarProcess objAsDownload = obj as AvatarProcess;
            if (objAsDownload == null)
                return false;
            else
                return Equals(objAsDownload);
        }
        public bool Equals(AvatarProcess download)
        {
            if (download == null)
                return false;
            return download.Id == Id;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public AvatarProcess()
        {

        }
        public AvatarProcess(VRCAvatarManager manager, object[] downloadMethodParams)
        {
            this.manager = manager;
            this.methodParams = downloadMethodParams;
        }
    }
}
