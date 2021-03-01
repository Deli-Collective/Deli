using System;

namespace Deli
{
	/// <summary>
	///		An event which can be subscribed and unsubscribed from, may only be evaluated once. After evaluation, subscriptions will throw an exception.
	/// </summary>
	/// <typeparam name="TDelegate">The type of delegate that this event is.</typeparam>
	public sealed class OneTimeEvent<TDelegate> where TDelegate : Delegate
	{
		private bool _ran;
		private TDelegate? _callback;

		/// <summary>
		///		Adds a subscription to the event.
		/// </summary>
		/// <param name="subscription">The delegate to subscribe.</param>
		/// <exception cref="InvalidOperationException">Event was already consumed.</exception>
		public void Add(TDelegate? subscription)
		{
			if (_ran)
			{
				throw new InvalidOperationException("Event was already run.");
			}

			_callback = (TDelegate?) Delegate.Combine(_callback, subscription);
		}

		/// <summary>
		///		Removes a subscription from the event.
		/// </summary>
		/// <param name="subscription">The delegate to unsubscribe.</param>
		/// <exception cref="InvalidOperationException">Event was already consumed.</exception>
		public void Remove(TDelegate? subscription)
		{
			if (_ran)
			{
				throw new InvalidOperationException("Event was already run.");
			}

			_callback = (TDelegate?) Delegate.Remove(_callback, subscription);
		}

		/// <summary>
		///		Consumes the event, preventing future subscription/unsubscription.
		/// </summary>
		/// <exception cref="InvalidOperationException">The event was already consumed.</exception>
		public TDelegate? Consume()
		{
			if (_ran)
			{
				throw new InvalidOperationException("Event was already run.");
			}

			_ran = true;

			var callback = _callback;
			_callback = null;

			return callback;
		}
	}
}
