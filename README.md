# Run multiple steps on multiple threads with uneven distribution
This is a proof of concept.

# Key points:
- Multiple threads per step
- Try to maximize : MaxItemsPerThread
- There is a queue between each step.
- When a thread is about to start a job, it will remove all the items it will use from the queue.
- When it has finished, it can add one or many items on the next queue.
- The data type of the object can change between steps.

# Why uneven distribution
Suppose you have access to multiple machines (via web api)
But, they don't have the same processor count / RAM / etc.
You can optimize the resources for speed.

# Copyright and license
Code released under the MIT license.
