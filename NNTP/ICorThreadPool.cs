// Mike Woodring
// http://staff.develop.com/woodring
//
using System.Runtime.InteropServices;

// To access the thread pool, either do this:
//
//   ICorThreadPool tp = (ICorThreadPool)new CorRuntimeHost();
//
// (the cast is required since interop shims like CorRuntimeHost
// cannot have methods, which would be required if it were to
// advertise that it implements ICorThreadPool statically).
//
// or this (a little cleaner):
//
//   ICorThreadPool tp = CLRThreadPool.Controller;
// 
//
[
// CLSID_CorRuntimeHost from MSCOREE.DLL
Guid("CB2F6723-AB3A-11D2-9C40-00C04FA30A3E"),
ComImport
]
public class CorRuntimeHost
{
}

public class CLRThreadPool
{
	public static ICorThreadPool Controller
	{
		get { return thePool; }
	}

	private static ICorThreadPool thePool = (ICorThreadPool)new CorRuntimeHost();
}

// The ICorThreadpool interface is documented (prototypes only) in
// mscoree.h, but is not made available from mscoree.tlb.  So the
// following interop stub lets us get our hands on the interface
// in order to query/control the CLR-managed thread pool.
//
// Because I'm only interested in adjusting the thread pool
// configuration, most of the members are actually invalid and
// cannot be called in their current form.
//
[
// IID_ICorThreadpool
Guid("84680D3A-B2C1-46e8-ACC2-DBC0A359159A"),
InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
public interface ICorThreadPool
{
	void RegisterWaitForSingleObject(); // DO NOT CALL - INCORRECT STACK FRAME
	void UnregisterWait(); // DO NOT CALL - INCORRECT STACK FRAME
	void QueueUserWorkItem(); // DO NOT CALL - INCORRECT STACK FRAME
	void CreateTimer(); // DO NOT CALL - INCORRECT STACK FRAME
	void ChangeTimer(); // DO NOT CALL - INCORRECT STACK FRAME
	void DeleteTimer(); // DO NOT CALL - INCORRECT STACK FRAME
	void BindIoCompletionCallback(); // DO NOT CALL - INCORRECT STACK FRAME
	void CallOrQueueUserWorkItem(); // DO NOT CALL - INCORRECT STACK FRAME
	void SetMaxThreads( uint MaxWorkerThreads, uint MaxIOCompletionThreads );
	void GetMaxThreads( out uint MaxWorkerThreads, out uint MaxIOCompletionThreads );
	void GetAvailableThreads( out uint AvailableWorkerThreads, out uint AvailableIOCompletionThreads );
}