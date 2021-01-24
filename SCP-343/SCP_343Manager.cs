using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCP_343
{
	public class SCP_343Manager : MonoBehaviour
	{
		public bool Is343 = false;
		public string PreviousBadgeName;
		public string PreviousBadgeColor;

		private ReferenceHub _hub;

		///Since this extends from <see cref="MonoBehaviour"/> this can use Unity functions like <see cref="Awake"/>
		public void Awake()
		{
			_hub = ReferenceHub.GetHub(this.gameObject);
		}

		public void FixedUpdate()
		{
			if (Is343)
				_hub.fpc.ModifyStamina(10000);
		}
	}
}
