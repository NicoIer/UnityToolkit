# MVVM
只推荐在UI中使用MVVM

核心思想是 View里的各个控件都定义一个数据对象，当修改这个数据对象是，控件自动刷新，当View里的控件做了操作时，ViewModel也更着更新

ViewModel和Model交互，用于同步底层数据