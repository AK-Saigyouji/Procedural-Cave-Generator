namespace AKSaigyouji.Modules.Outlines
{
    public abstract class OutlineModule : Module
    {
        protected const string fileName = "OutlinePrefabber";
        protected const string rootMenupath = "Cave Generation/Outline Prefabbers/";

        public abstract IOutlinePrefabber GetOutlinePrefabber();
    } 
}