#pragma once

#include <atomic>

namespace Emesary
{
	/// Class from which a class that wants to be used as an ObjectPtr must inherit from;
	class ObjectRef {
	public:
		ObjectRef(void) : _refcount(0u) { }

		/// Do not copy reference counts. Each new object has it's own counter
		ObjectRef(const ObjectRef&) : _refcount(0u) { }
		ObjectRef& operator=(const ObjectRef&) { return *this; }

		static size_t add(const ObjectRef* ref)
		{
			if (ref)
				return ++(ref->_refcount);
			else
				return 0;
		}

		static size_t release(const ObjectRef* ref) noexcept
		{
			if (ref)
				return --(ref->_refcount);
			else
				return 0;
		}
	private:
		mutable std::atomic<size_t> _refcount;
	};
}