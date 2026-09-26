using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Platform.Helper
{
    public class PwdHelper
    {
        //定义 注册 包装
        public static string GetCustPwd(DependencyObject obj)
        {
            return (string)obj.GetValue(CustPwdProperty);
        }

        public static void SetCustPwd(DependencyObject obj, string value)
        {
            obj.SetValue(CustPwdProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustPwdProperty =
            DependencyProperty.RegisterAttached("CustPwd", typeof(string), typeof(PwdHelper),
                new PropertyMetadata("", OnCustPwdChanged)
                );

        #region CustPwd改变了，Passwrod跟着变
        /// <summary>
        /// 附加属性(密码)值改变回调
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnCustPwdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string newPwd = (string)e.NewValue;

            PasswordBox passBox = d as PasswordBox;//弱转化
            if (passBox != null)
            {
                passBox.Password = newPwd;

                //将光标移动最后
                SetSelection(passBox, passBox.Password.Length, 0);
            }
        }
        #endregion

        #region Password变了，CustPwd跟着变。想办法订阅PasswordChanged
        public static bool GetIsOpen(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsOpenProperty);
        }

        public static void SetIsOpen(DependencyObject obj, bool value)
        {
            obj.SetValue(IsOpenProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.RegisterAttached("IsOpen", typeof(bool), typeof(PwdHelper), new PropertyMetadata(false, OnIsOpenChanged));

        /// <summary>
        /// 订阅PasswordChanged事件
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //订阅PasswordChanged
            PasswordBox passBox = d as PasswordBox;
            if (passBox != null)
            {
                passBox.PasswordChanged -= OnPasswordChanged;//取消订阅
                passBox.PasswordChanged += OnPasswordChanged;//订阅
            }
        }

        /// <summary>
        /// 将Password赋给CuetPwd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pd = (PasswordBox)sender;//PasswordBox:DependencyObject
            SetCustPwd((DependencyObject)sender, pd.Password);//将Password赋给CuetPwd
        }

        #endregion


        /// <summary>
        /// 设置光标位置
        /// </summary>
        /// <param name="passwordBox"></param>
        /// <param name="start">光标开始位置</param>
        /// <param name="length">选中长度</param>
        private static void SetSelection(PasswordBox passwordBox, int start, int length)
        {
            passwordBox.GetType()
                       .GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                       .Invoke(passwordBox, new object[] { start, length });
        }
    }
}
