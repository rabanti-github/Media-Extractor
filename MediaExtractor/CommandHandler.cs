/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2020
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Windows.Input;

namespace MediaExtractor
{
    
    /// <summary>
    /// Class to handle command, sent by WPF controls
    /// </summary>
    public class CommandHandler : ICommand
    {
        private Action action;
        private Func<bool> canExecute;

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="action"></param>
        /// <param name="canExecute"></param>
        public CommandHandler(Action action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            action();
        }
    }
}