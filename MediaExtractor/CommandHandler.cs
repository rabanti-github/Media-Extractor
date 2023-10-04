/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
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
        private readonly Action action;
        private readonly Func<bool> canExecute;

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="action">Action object</param>
        /// <param name="canExecute">Function reference to determine whether the action can be executed</param>
        public CommandHandler(Action action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Handles changing of the method for determination of the execution 
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// gets whether the command can be executed
        /// </summary>
        /// <param name="parameter">parameter object</param>
        /// <returns>True if command can be executed, otherwise false</returns>
        public bool CanExecute(object parameter)
        {
            return canExecute.Invoke();
        }

        /// <summary>
        /// Method to perform the defined action
        /// </summary>
        /// <param name="parameter">Parameter object</param>
        public void Execute(object parameter)
        {
            action();
        }
    }
}