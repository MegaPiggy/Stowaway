using OWML.Utils;
using UnityEngine;

namespace Stowaway
{
	public static class Teleportation
	{
		private static OWRigidbody getPlayerBody()
		{
			return Locator.GetPlayerBody();
		}

		public static void teleportPlayerTo(OWRigidbody teleportTo, Vector3 position, Vector3 velocity, Vector3 angularVelocity, Vector3 acceleration, Quaternion rotation)
		{
			if (teleportTo)
			{
				teleportObjectTo(getPlayerBody(), teleportTo, position, velocity, angularVelocity, acceleration, rotation);
				GlobalMessenger.FireEvent("WarpPlayer");
			}
		}

		public static void teleportPlayerTo(Vector3 position, Vector3 velocity, Vector3 angularVelocity, Vector3 acceleration, Quaternion rotation)
		{
			teleportObjectTo(getPlayerBody(), position, velocity, angularVelocity, acceleration, rotation);
			GlobalMessenger.FireEvent("WarpPlayer");

		}

		public static void teleportObjectTo(OWRigidbody teleportObject, OWRigidbody teleportTo, Vector3 position, Vector3 velocity, Vector3 angularVelocity, Vector3 acceleration, Quaternion rotation)
		{
			if (teleportTo)
			{
				var newPosition = teleportTo.transform.TransformPoint(position);
				var newVelocity = velocity - teleportTo.GetPointTangentialVelocity(newPosition) + teleportTo.GetVelocity();
				var newAnglarVelocity = angularVelocity + teleportTo.GetAngularVelocity();
				var newAcceleration = acceleration + teleportTo.GetAcceleration();
				var newRotation = rotation * teleportTo.GetRotation();
				teleportObjectTo(teleportObject, newPosition, newVelocity, newAnglarVelocity, newAcceleration, newRotation);
			}
		}

		public static void teleportObjectTo(OWRigidbody teleportObject, Vector3 position, Vector3 velocity, Vector3 angularVelocity, Vector3 acceleration, Quaternion rotation)
		{
			teleportObject.SetPosition(new Vector3(position.x, position.y, position.z));
			teleportObject.SetVelocity(new Vector3(velocity.x, velocity.y, velocity.z));
			teleportObject.SetRotation(new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w));
			teleportObject.SetAngularVelocity(new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));

			teleportObject.SetValue("_lastPosition", new Vector3(position.x, position.y, position.z));
			teleportObject.SetValue("_currentVelocity", new Vector3(velocity.x, velocity.y, velocity.z));
			teleportObject.SetValue("_lastVelocity", new Vector3(velocity.x, velocity.y, velocity.z));
			teleportObject.SetValue("_currentAngularVelocity", new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));
			teleportObject.SetValue("_lastAngularVelocity", new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));
			teleportObject.SetValue("_currentAccel", new Vector3(acceleration.x, acceleration.y, acceleration.z));
			teleportObject.SetValue("_lastAccel", new Vector3(acceleration.x, acceleration.y, acceleration.z));
		}
	}
}