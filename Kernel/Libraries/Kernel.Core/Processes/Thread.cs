﻿#define THREAD_TRACE
#undef THREAD_TRACE

using System;

namespace Kernel.Core.Processes
{
    public delegate void ThreadStartMethod();
        
    public unsafe class Thread : FOS_System.Object
    {
        public uint Id;
        
        public ThreadState* State;

        /// <remarks>
        /// Units of [time period of scheduler]
        /// </remarks>
        public int TimeToRun;
        /// <remarks>
        /// Units of [time period of scheduler]
        /// </remarks>
        public int TimeToRunReload;

        /// <remarks>
        /// Units of ms
        /// </remarks>
        public int TimeToSleep = 0;
        
        public Thread(ThreadStartMethod StartMethod, uint AnId, bool UserMode)
        {
#if THREAD_TRACE
            Console.Default.WriteLine(" > > > Constructing thread object...");
#endif
            //Init thread state
#if THREAD_TRACE
            Console.Default.WriteLine(" > > > Allocating state memory...");
#endif
            State = (ThreadState*)FOS_System.Heap.Alloc((uint)sizeof(ThreadState));

            // Init Id and EIP
            //  Set EIP to the first instruction of the main method
#if THREAD_TRACE
            Console.Default.WriteLine(" > > > Setting info...");
#endif
            Id = AnId;
            State->StartEIP = (uint)Utilities.ObjectUtilities.GetHandle(StartMethod);

            // Allocate kernel memory for the kernel stack for this thread
            //  Used when this thread is preempted or does a sys call. Stack is switched to
            //  this thread-specific kernel stack
#if THREAD_TRACE
            Console.Default.WriteLine(" > > > Allocating kernel stack...");
#endif
            State->KernelStackTop = (byte*)FOS_System.Heap.Alloc(0x1000, 4) + 0xFFC; //4KiB, 4-byte aligned
            
            // Allocate free memory for the user stack for this thread
            //  Used by this thread in normal execution
#if THREAD_TRACE
            Console.Default.WriteLine(" > > > Mapping thread stack page...");
#endif
            State->UserMode = UserMode;
            State->ThreadStackTop = (byte*)Hardware.VirtMemManager.MapFreePage(
                UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None :
                           Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly) + 4092; //4 KiB, page-aligned
            
            // Set ESP to the top of the stack - 4 byte aligned, high address since x86 stack works
            //  downwards
#if THREAD_TRACE
            Console.Default.WriteLine(" > > > Setting ESP...");
#endif
            State->ESP = (uint)State->ThreadStackTop;

            // TimeToRun and TimeToRunReload are set up in Scheduler.InitProcess which
            //      is called when a process is registered.

            // Init SS
            //  Stack Segment = User or Kernel space data segment selector offset
            //  Kernel data segment selector offset (offset in GDT) = 0x10 (16)
            //  User   data segment selector offset (offset in GDT) = 0x23 (32|3)
            //          User data segment selector must also be or'ed with 3 for User Privilege level
#if THREAD_TRACE
            Console.Default.WriteLine(" > > > Setting SS...");
#endif
            State->SS = UserMode ? (ushort)0x23 : (ushort)0x10;

            // Init Started
            //  Not started yet so set to false
#if THREAD_TRACE
            Console.Default.WriteLine(" > > > Setting started...");
#endif
            State->Started = false;
        }

        public static void Sleep(int ms)
        {
            Scheduler.Disable();
            ProcessManager.CurrentThread.TimeToSleep = ms /* x * 1ms / [Scheduler period in ns] = x * 1 = x */;
            ProcessManager.CurrentThread.TimeToRun = 0;
            Scheduler.Enable();
            // Busy wait for the scheduler to interrupt the thread, sleep it and
            //  then as soon as the sleep is over this condition will go false
            //  so the thread will continue
            while (ProcessManager.CurrentThread.TimeToSleep > 0)
            {
                ;
            }
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ThreadState
    {
        /* Do not re-order the fields in the structure. */

        public bool Started;            // Offset: 0
        
        public uint ESP;                // Offset: 1
        public ushort SS;               // Offset: 5
        public byte* KernelStackTop;    // Offset: 7
        public byte* ThreadStackTop;    // Offset: 11
        
        public uint StartEIP;           // Offset: 15
        public bool Terminated;         // Offset: 19

        public bool UserMode;           // Offset: 20
    }
}
