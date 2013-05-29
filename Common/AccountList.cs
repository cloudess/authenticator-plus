using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Authenticator
{
    public class AccountList : ObservableCollection<Account>
    {
        public AccountList()
            : base()
        {
        }
    }
}