namespace Nico
{
    //必须 有一个无参构造函数 用于 new  必须是 class 不能是 struct
    public interface IModel
    {
        public void OnRegister();
        public void OnSave();
    }
}