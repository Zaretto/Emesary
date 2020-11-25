#pragma once

#include <atomic>

namespace Emesary
{
	template <typename T>
	class ObjectPtr
	{
	private:
		T* referred_object;

	public:
		ObjectPtr(void) noexcept : referred_object(0)
		{
		}

		ObjectPtr(T* t) : referred_object(t)
		{
			get(referred_object);
		}

		ObjectPtr(const ObjectPtr& p) : referred_object(p.get())
		{
			get(referred_object);
		}

		ObjectPtr(ObjectPtr&& other) noexcept
			: ObjectPtr()
		{
			swap(other);
		}

		template<typename U>
		ObjectPtr(const ObjectPtr<U>& t) : referred_object(t.get())
		{
			get(referred_object);
		}

		virtual ~ObjectPtr()
		{
			reset();
		}

		void reset() noexcept
		{
			if (referred_object && !T::release(referred_object))
				delete referred_object;

			referred_object = 0;
		}
		ObjectPtr& operator=(const ObjectPtr& p)
		{
			reset(p.get()); return *this;
		}

		// These operators and methods will return something that should only be used transiently as the return 
		// pointer is not guaranteed to be valid after a scope change. If you need something that lasts
		// longer then make a copy of the managed object. [max adds: bbfdaz}
		T* operator->() const
		{
			return referred_object;
		}

		T& operator*() const
		{
			return *referred_object;
		}

		T* get_referred_object() const
		{
			return referred_object;
		}

		T* get(void) const
		{
			return referred_object;
		}

		void reset(T* p)
		{
			ObjectPtr(p).swap(*this);
		}
		void swap(ObjectPtr& other) noexcept
		{
			// no except swap
			using std::swap;
			static_assert(noexcept(swap(referred_object, other.referred_object)), "this swap() is not 'noexcept'");
			swap(referred_object, other.referred_object);
		}
		// returns true if both objects refer to the same data
		bool is_equal(const ObjectPtr<T>& m1) const
		{
			return this->referred_object == m1.referred_object;
		}
		bool operator == (T& m1) const { return this->referred_object == &m1; }
		bool operator == (const T& m1) const { return this->referred_object == &m1; }
		bool operator == (T* m1) const { return this->referred_object == m1; }
		bool operator == (const T* m1) const { return this->referred_object == m1; }
		bool operator != (T* m1) const { return this->referred_object != m1; }
	private:
		void get(const T* p) const
		{
			T::add(p);
		}
	};


	/*
	* The comparison operators are really a convenience feature, and allows
	* managed object to be treated a little like a reference, without affecting underlying
	* code
	*/
	template <class T> inline bool operator==(const ObjectPtr<T>& a, const ObjectPtr<T>& b)
	{
		return a.is_equal(b);
	}

	template <class T> inline bool operator!=(const ObjectPtr<T>& a, const ObjectPtr<T>& b)
	{
		return !a.is_equal(b);
	}

	template<class T, class U>
	bool operator==(const ObjectPtr<T>& lhs, const ObjectPtr<U>& rhs)
	{
		return lhs.get() == rhs.get();
	}

	/**
	 * Compare two ObjectPtr<T> objects for equality.
	 *
	 * @note Only pointer values are compared, not the actual objects they are
	 *       pointing at.
	 */
	template<class T, class U>
	bool operator!=(const ObjectPtr<T>& lhs, const ObjectPtr<U>& rhs)
	{
		return lhs.get() != rhs.get();
	}
}