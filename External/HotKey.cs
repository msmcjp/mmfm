﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Diagnostics.Contracts;
using System.Windows.Input;

/// <summary>
/// This class is modified: https://github.com/mok-aster/GlobalHotKey.NET/blob/master/src/GlobalHotKey/HotKey.cs
/// </summary>
namespace HotKey
{
    internal class HotKeyWinApi
    {
        public const int WmHotKey = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    /// <summary>
    /// グローバルホットキーを登録機能を提供します。
    /// </summary>
	public sealed class HotKey : IDisposable
    {
        // ホットキーが押された時には発生するイベントです。
        public event Action<HotKey> Pressed;

        private static int count;
        private int id;
        private bool isKeyRegistered;
        private readonly IntPtr handle;

        /// <summary>
        /// ホットキーに使用されるキー
        /// </summary>
		public Key Key { get; private set; }

        /// <summary>
        /// ホットキーに使用される修飾キー
        /// </summary>
		public ModifierKeys ModifierKey { get; private set; }

        /// <summary>
        /// 新規のホットキーを登録します。
        /// </summary>
        /// <param name="window">ウィンドウ</param>
        /// <param name="keyGesture">キーの組み合わせ</param>
        public HotKey(System.Windows.Window window, KeyGesture keyGesture)
        {
            var handle = new WindowInteropHelper(window).Handle;
            Contract.Requires(keyGesture.Modifiers != ModifierKeys.None || keyGesture.Key != Key.None);
            Contract.Requires(handle != IntPtr.Zero);

            this.Key = keyGesture.Key;
            this.ModifierKey = keyGesture.Modifiers;
            this.id = ++count;
            this.handle = handle;

            RegisterHotKey();

            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        private void RegisterHotKey()
        {
            if (Key == Key.None)
            {
                return;
            }

            UnregisterHotKey();

            isKeyRegistered = HotKeyWinApi.RegisterHotKey(handle, id, ModifierKey, (uint)KeyInterop.VirtualKeyFromKey(Key));
            if (!isKeyRegistered)
            {
                throw new ApplicationException(Mmfm.Properties.Resources.HoKey_InUse);
            }
        }

        private void UnregisterHotKey()
        {
            isKeyRegistered = !HotKeyWinApi.UnregisterHotKey(handle, id);
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == HotKeyWinApi.WmHotKey && (int)(msg.wParam) == id)
                {
                    OnHotKeyPressed();
                    handled = true;
                }
            }
        }

        private void OnHotKeyPressed()
        {
            Pressed?.Invoke(this);
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;

            ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;

            UnregisterHotKey();
        }
    }
}