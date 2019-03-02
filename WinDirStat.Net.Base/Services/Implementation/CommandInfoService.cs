using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Services.Implementation {
	public abstract class CommandInfoService {

		private readonly Dictionary<string, IRelayUICommandInfo> commandInfos;


		public CommandInfoService() {
			commandInfos = new Dictionary<string, IRelayUICommandInfo>();
		}

		public void Initialize() {
			foreach (PropertyInfo prop in GetType().GetProperties()) {
				if (prop.PropertyType == typeof(IRelayUICommandInfo)) {
					commandInfos.Add(prop.Name, (IRelayUICommandInfo) prop.GetValue(this));
				}
			}
		}

		public IRelayUICommandInfo this[string commandName] => commandInfos[commandName];

	}
}
