using System;
using System.Collections.Generic;
using System.Linq;

namespace TM.Shared.Parse
{
   public abstract class CallResultBase
   {
      /// <summary>
      /// Constructor that takes whether the result is successful
      /// </summary>
      /// <param name="success"></param>
      protected CallResultBase(bool success)
      {
         Succeeded = success;
         Errors = Enumerable.Empty<string>();
      }

      /// <summary>
      /// Failure constructor that takes error messages
      /// </summary>
      /// <param name="errors"></param>
      protected CallResultBase(IEnumerable<string> errors)
      {
         Succeeded = false;
         Errors = errors;
      }


      /// <summary>
      /// Failure constructor that takes error messages
      /// </summary>
      /// <param name="errors"></param>
      protected CallResultBase(params string[] errors)
      {
         Succeeded = false;
         Errors = errors;
      }


      /// <summary>
      /// List of errors
      /// </summary>
      public IEnumerable<string> Errors { get; protected set; }

      public Exception Exception { get; protected set; }

      //
      // Summary:
      //     True if the operation was successful
      public bool Succeeded { get; protected set; }

   }


   public class CallResult<TResult> : CallResultBase
   {
      protected CallResult(bool success, TResult resultValue)
         : base(success)
      {
         Value = resultValue;
      }

      public CallResult(IEnumerable<string> errors)
         : base(errors)
      {
      }

      public CallResult(params string[] errors)
         : base(errors)
      {
      }


      public TResult Value { get; protected set; }


      /// <summary>
      /// Failed helper method
      /// </summary>
      /// <param name="errors"></param>
      /// <returns></returns>
      public static CallResult<TResult> Failed(params string[] errors)
      {
         return new CallResult<TResult>(errors);
      }


      /// <summary>
      /// Successed helper method
      /// </summary>
      /// <param name="resultValue"></param>
      /// <returns></returns>
      public static CallResult<TResult> Success(TResult resultValue)
      {
         return new CallResult<TResult>(true, resultValue);
      }
   }


   /// <summary>
   /// Represents the result of an call
   /// </summary>

   public class CallResult : CallResultBase
   {
      /// <summary>
      /// Constructor that takes whether the result is successful
      /// </summary>
      /// <param name="success"></param>
      protected CallResult(bool success)
         : base(success)
      {
      }

      /// <summary>
      /// Failure constructor that takes error messages
      /// </summary>
      /// <param name="errors"></param>
      public CallResult(IEnumerable<string> errors)
         : base(errors)
      {
      }


      /// <summary>
      /// Failure constructor that takes error messages
      /// </summary>
      /// <param name="errors"></param>
      public CallResult(params string[] errors)
         : base(errors)
      {
      }

      /// <summary>
      /// Failed helper method
      /// </summary>
      /// <param name="errors"></param>
      /// <returns></returns>
      public static CallResult Failed(params string[] errors)
      {
         return new CallResult(errors);
      }

      public static CallResult Failed(Exception exception)
      {
         return new CallResult
         {
            Succeeded = false,
            Exception = exception,
            Errors = new[] { exception.ToString() }
         };
      }


      /// <summary>
      /// Static success result
      /// </summary>
      public static CallResult Success
      {
         get { return new CallResult(success: true); }
      }
   }
}
