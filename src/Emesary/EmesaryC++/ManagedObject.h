#pragma once

/*
* Class Name      	   : template <class T> ManagedObject
* Description	       : Class to implement a reference counted lifecycle managed object.
*            	       : The class contains two pointers, one for the data and one for the count.
*            	       : Both of these pointers are shared by all references of the shared managed
*            	       : object.
*                     :
*                     : This class is really designed to manage 'whole' objects, rather than pointers to objects
*                     : basically because of the ownership difficulties that can arise when attempting to reference
*                     : pointers allocated elsewhere. The class does however support this, but when passing a pointer
*                     : please ensure that the object has been allocated with new, and responsibility for deletion
*                     : passes to this object.
*                     :
*                     : Usage exmaples:
*                     : - When you have an object or structure, you can create a managed reference counted instance by
*                          using ManagedObject<MyClass> tsc(s); // Contains something
*                     : - When you have a managed object that you need elsewhere then you can use
*                         ManagedObject<MyClass> mo_MyClass = tsc; // even after tsc goes out of scope this will be valid.
*/
template <class T>
class ManagedObject
{
private:
	mutable int *referring_count;
	T   *referred_object;

public:
public:
	// Empty object ready for copying etc.
	ManagedObject()
		: referred_object(0), referring_count(0)
	{
	}

	// Manage something already allocated
	ManagedObject(T *t)
		: referred_object(t), referring_count(new int(1))
	{
	}

	// Create a copy of the object and manage this
	ManagedObject(const T &t) : referred_object(0), referring_count(new int(1))
	{
		copy(&t);
	}

	// Create a copy of a manage object, and share the data
	ManagedObject(const ManagedObject& h)
	{
		referred_object = h.referred_object; // possibly null, but to no matter
		referring_count = h.referring_count;

		// It is possible that the referring object is null, in which case there is no need
		// to allocate anything, so just return.
		if (!h.referred_object)
			return;

		// this should never happen, i.e. referring count should always point to something
		// when referred object is non-null.
		if (!h.referring_count)
		{
			referring_count = h.referring_count = new int;
			*referring_count = *h.referring_count = 1;
		}

		(*referring_count)++;
	}

	~ManagedObject()
	{
		// If the data we are managing 
		if (referring_count && --(*referring_count) == 0)
		{
			delete referred_object;
			delete referring_count;
		}
	}

	ManagedObject<T>& operator=(const ManagedObject<T>& h)
	{
		if (referred_object == h.referred_object)
		{
			return *this;
		}

		if (referring_count && --(*referring_count) == 0)
		{
			delete referred_object;
			delete referring_count;
		}
		referred_object = h.referred_object;
		referring_count = h.referring_count;
		(*referring_count)++;
		return *this;
	}

	// These operators and methods will return something that should only be used transiently as the return 
	// pointer is not guaranteed to be valid after a scope change. If you need something that lasts
	// longer then make a copy of the managed object. [max adds: bbfdaz}
	T *operator->() const
	{
		return referred_object;
	}

	T& operator*() const
	{
		return *referred_object;
	}

	T *get_referred_object() const
	{
		return referred_object;
	}

	/*
	* Make an allocated copy of an object
	*/
	void copy(const T* t)
	{
		if (t != referred_object)
		{
			/*
			* If the first(last) reference then reset and free when required.
			*/
			if (--*referring_count == 0)
			{
				*referring_count = 1;
				if (referred_object)
					delete referred_object;
			}
			else
			{
				referring_count = new int(1);
			}
			referred_object = new T(*t);
		}
	}

	// returns true if both objects refer to the same data
	bool is_equal(const ManagedObject<T>& m1) const
	{
		return this->referred_object == m1.referred_object;
	}
};

/*
* The comparison operators are really a convenience feature, and allows
* managed object to be treated a little like a reference, without affecting underlying
* code
*/
template <class T> inline bool operator==(const ManagedObject<T>& a, const ManagedObject<T>& b)
{
	return a.is_equal(b);
}

template <class T> inline bool operator!=(const ManagedObject<T>& a, const ManagedObject<T>& b)
{
	return !a.is_equal(b);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////
