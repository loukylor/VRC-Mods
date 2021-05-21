namespace AvatarDownloadPriority
{
    public class AvatarDownload
    {
        public VRCAvatarManager manager;
        public object[] downloadMethodParams;
        public string Id => manager.field_Private_VRCPlayer_0.prop_Player_0.prop_APIUser_0.id;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            AvatarDownload objAsDownload = obj as AvatarDownload;
            if (objAsDownload == null)
                return false;
            else
                return Equals(objAsDownload);
        }
        public bool Equals(AvatarDownload download)
        {
            if (download == null)
                return false;
            return download.manager.GetInstanceID() == manager.GetInstanceID();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public AvatarDownload()
        {

        }
        public AvatarDownload(VRCAvatarManager manager, object[] downloadMethodParams)
        {
            this.manager = manager;
            this.downloadMethodParams = downloadMethodParams;
        }
    }
}
